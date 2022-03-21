const { Client, Intents } = require('discord.js');
const client = new Client({ intents: [Intents.FLAGS.GUILDS] });
const dotenv = require('dotenv');

dotenv.config();

client.on('ready', () => {
	console.log('Hey!');
});

client.login(process.env.DISCORD_TOKEN);