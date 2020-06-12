    /*
    ScriptsManager, Administrador de scripts
    Copyright (C) 2020 Erick Mora

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

    erickfernandomoraramirez@gmail.com
    erickmoradev@gmail.com
    https://dev.moradev.dev/myportfolio
    */
const http = require('http');

let serverPort = process.env.MY_IPC_SERVER_PORT || "40010";
let clientPort = process.env.MY_IPC_CLIENT_PORT || "40110";
let includePath = process.env.NODEJS_INCLUDE_PATH || ".";
let testMode = process.env.SCRIPTS_MANAGER_TEST_MODE || false;

if (testMode) {
	console.log(`Server Port: ${serverPort};`);
	console.log(`Client Port: ${clientPort};`);
	console.log(`Include Path: ${includePath};`);
	console.log('Testing include path...');
	try {
		let t = require(`${includePath}\\MyIpc`);
		console.log('sucessfully');
		process.exit(0);
	} catch (e) {
		console.log(`fail: ${e}`);
		process.exit(1);
	}
}

const MyIpc = require(`${includePath}\\ScriptsManager`);

let ipc = new MyIpc(serverPort, clientPort, (d) => {
	console.log(d);
});

let port = 8090;

//Setting a "MyIpcLabel" named "label" control in the Scripts Manager,
//And setting his text to the listen port
//[NOT USE ', : or ;]
let message = `I am listening in the port ${port}`;
console.log(message);

let requestCount = 0;
let requestMax = 10;

http.createServer((req, res) => {
	res.writeHead(200, { "Content-Type": "text/html" });
	console.log(req.url);
	res.end("Success");
	requestCount++;
	ipc.setControl('progress', 'MyIpcLable', {
		"Text": `request ${requestCount}`
	});
}).listen(port);
