const dotenv = require('dotenv');

const discordJs = require('./discord_js_setup');
const database = require('./backend/database');

// .env config
dotenv.config();

database.setupDatabaseConnection(process.env.MYSQL_HOST, process.env.MYSQL_USERNAME, process.env.MYSQL_PASSWORD, process.env.MYSQL_DB);
database.startDatabaseConnection();

discordJs.setup(process.env.DISCORD_TOKEN);

let a = database.addNewMod("test", "test", "test", "test");
console.log(`val: ${a}`);