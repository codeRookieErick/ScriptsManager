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