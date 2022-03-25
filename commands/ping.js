const { SlashCommandBuilder } = require('@discordjs/builders');

module.exports = {
	data: new SlashCommandBuilder()
		.setName('ping')
		.setDescription('Returns the current ping of the bot.'),
	async execute(interaction) {
        const sent = await interaction.reply({ content: 'Pinging...', fetchReply: true });
        interaction.editReply(`Current backend server ping: ${sent.createdTimestamp - interaction.createdTimestamp}ms (High values may be a Discord API issue).`);
	},
};