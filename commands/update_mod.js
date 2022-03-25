const { SlashCommandBuilder } = require('@discordjs/builders');
const { getModData, updateMod, updateModDownloads, doesModVersionExist } = require('../backend/database');
const { modUploadRoleId } = require("../config.json");

module.exports = {
	data: new SlashCommandBuilder()
		.setName('update_mod')
		.setDescription('Updates an existing mod')
        .addStringOption(option => option.setName("mod_id").setDescription("The mod ID of the mod").setRequired(true))
        .addStringOption(option => option.setName("version").setDescription("The new version of the mod. Has to be identical as the one in the Mod assembly!").setRequired(true))
        .addStringOption(option => option.setName("download_link").setDescription("The direct download link for the mod.").setRequired(true)),

	async execute(interaction) {
        await interaction.deferReply();

        if (!interaction.member.roles.cache.some(role => role.id == modUploadRoleId)) {
            interaction.editReply("No permission!");
            return; 
        }

        let mod_id = interaction.options.getString("mod_id");
        let tempData = await getModData(mod_id);

        if(tempData == undefined){ // If mod does not exist exists.
            await interaction.editReply("The mod ID is not registered on the database!");
            return;
        }

        let version = interaction.options.getString("version");
        let download_link = interaction.options.getString("download_link");
        
        // Check if mod version is not being reused.
        let reuseSafetyCheck = await doesModVersionExist(tempData["internal_id"], version);
        
        if(reuseSafetyCheck != undefined){
            await interaction.editReply("This mod already has this version registered on the database. Reusing versions is not allowed.");
            return;
        }

        let result = await updateMod(mod_id, version, download_link);

        if(result === true){
            await updateModDownloads(tempData["internal_id"], version);
            await interaction.editReply(`${mod_id} has been succesfully updated.`);
        } else {
            await interaction.editReply("Seems like something went wrong internally.");
        }
	},
};