const net = require('net');

module.exports = function(serverPort, clientPort, callback){
    this.callback = callback || ((i, d)=>{});
    this.serverPort = serverPort;
    this.clientPort = clientPort;
    this.server = net.createServer(this.serverCallback);
    this.serverCallback = (server) => {
        server.on('data', (d) => this.callback(this, d));
        server.on('error', (d) => this.callback(this, 'Error'));
    }

    this.listen = () => {
        this.server.listen(this.clientPort);
    }

    this.send = (data) => {
        net.connect({'port':this.serverPort}).on('error', (e)=>{
            ///console.log(e);
        }).end(data);
    }

    this.setControl = (name, controlType, values) => {
        this.send(`create-control ${controlType} ${name};`);
        for(let i in values){
            let propertySet = `'set-${i}=${values[i]}'`;
            this.send(`change-control ${name} ${propertySet};`);
        }
    }
}