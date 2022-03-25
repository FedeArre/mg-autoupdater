const { SlashCommandBuilder } = require('@discordjs/builders');
const { updateAutoupdater } = require('../backend/database');
const { autoupdaterManagerRoleId } = require("../config.json");

module.exports = {
	data: new SlashCommandBuilder()
		.setName('update_autoupdater')
		.setDescription('Updates the autoupdater')
        .addStringOption(option => option.setName("version").setDescription("The new version of the autoupdater.").setRequired(true))
        .addStringOption(option => option.setName("download_link").setDescription("The direct download link for the update.").setRequired(true)),

	async execute(interaction) {
        await interaction.deferReply();

        if (!interaction.member.roles.cache.some(role => role.id == autoupdaterManagerRoleId)) {
            interaction.editReply("No permission!");
            return; 
        }

        let version = interaction.options.getString("version");
        let download_link = interaction.options.getString("download_link");

        let result = await updateAutoupdater(version, download_link);

        if(result === true){
            await interaction.editReply(`The autoupdater has been succesfully updated.`);
        } else {
            await interaction.editReply("Seems like something went wrong internally.");
        }
	},
};