const dotenv = require('dotenv');

const discordJs = require('./discord_js_setup');

// .env config
dotenv.config();

discordJs.setup(process.env.DISCORD_TOKEN);