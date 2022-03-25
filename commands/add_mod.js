const { SlashCommandBuilder } = require('@discordjs/builders');
const { getModData, addNewMod } = require('../backend/database');
const { modUploadRoleId } = require("../config.json");

module.exports = {
	data: new SlashCommandBuilder()
		.setName('add_mod')
		.setDescription('Adds a new mod into the database')
		.addStringOption(option => option.setName("mod_name").setDescription("The name of the mod").setRequired(true))
        .addStringOption(option => option.setName("mod_id").setDescription("The mod ID of the mod. Has to be the same as the ID on the Mod assembly!").setRequired(true))
        .addStringOption(option => option.setName("version").setDescription("The current version of the mod. Has to be identical as the one in the Mod assembly!").setRequired(true))
        .addStringOption(option => option.setName("download_link").setDescription("The direct download link for the mod.").setRequired(true))
        .addStringOption(option => option.setName("file_name").setDescription("The file name of your mod. You have to include the extension.").setRequired(true)),

	async execute(interaction) {
        await interaction.deferReply();
        
        if (!interaction.member.roles.cache.some(role => role.id == modUploadRoleId)) {
            interaction.editReply("No permission!");
            return; 
        }

        let mod_id = interaction.options.getString("mod_id");
        let tempData = await getModData(mod_id);

        if(tempData != undefined){ // If mod already exists.
            await interaction.editReply("The mod ID is already being used!");
            return;
        }

        let mod_name = interaction.options.getString("mod_name");
        let version = interaction.options.getString("version");
        let download_link = interaction.options.getString("download_link");
        let file_name = interaction.options.getString("file_name");
        
        let result = await addNewMod(mod_id, mod_name, version, download_link, file_name, interaction.user.id);

        if(result === true){
            await interaction.editReply(`${mod_id} was succesfully added into the database.`);
        } else {
            await interaction.editReply("Seems like something went wrong internally.");
        }
	},
};