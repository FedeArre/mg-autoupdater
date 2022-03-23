const dotenv = require('dotenv');

const discordJs = require('./discord_js_setup');
const express_handler = require('./backend/express_handler');

// .env config
dotenv.config();

discordJs.setup(process.env.DISCORD_TOKEN);