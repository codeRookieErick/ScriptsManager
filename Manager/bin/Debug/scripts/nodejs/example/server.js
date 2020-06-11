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

const MyIpc = require(`${includePath}\\MyIpc`);

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
