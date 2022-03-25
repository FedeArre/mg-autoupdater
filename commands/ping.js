const { SlashCommandBuilder } = require('@discordjs/builders');

module.exports = {
	data: new SlashCommandBuilder()
		.setName('ping')
		.setDescription('Returns the current ping of the bot.'),
	async execute(interaction) {
		return interaction.reply(`Current ping is ${client.ws.ping}ms.`);
	},
};