let http = require('http');
let fs = require('fs');
fs.writeFile('perro.txt', `ran`, (d)=>{});


let serverPort = process.env.MY_IPC_SERVER_PORT || "40010";
let clientPort = process.env.MY_IPC_CLIENT_PORT || "40110";
let includePath = process.env.NODE_INCLUDE_PATH || ".";

const Worker = require(`${includePath}\\Worker`).Worker;
const Control = require(`${includePath}\\Worker`).Control;
const MyRouter = require(`${includePath}\\MyRouter`);
const MyModuleWatcher = require(`${includePath}\\MyModuleWatcher`);


let ipc = new Worker((i, d) =>{
    ipc.print(d);
});

let myIpcLabel = new Control("MyIpcLabel", "myIpcLabel");
ipc.addControl(myIpcLabel);
let requestCount = 0;


const watcher = new MyModuleWatcher();

let port = 80;
let server = new MyRouter();
watcher.watch("app", (app) => {
	app(server);
	let message = `App updated ${new Date().toString()}`;
	//console.log(message);
	myIpcLabel.setProperty("Text", message);
	ipc.print(message);
	server.addHandler((req, res, params)=>{
		myIpcLabel.setProperty("Text", `Request: ${req.url}`);
		//console.log(req,url);
		ipc.print(`'Request: ${req.url}'`);
	});
});
server.listen(port);
myIpcLabel.setProperty("Text", `Listening ${port}`);
//console.log('eo');