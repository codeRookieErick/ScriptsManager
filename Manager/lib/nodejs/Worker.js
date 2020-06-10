let MyIpc = require('./MyIpc').MyIpc;

exports.Worker = function(callback) {
    this.callback = callback || ((i, d)=>{});
    this.controls = []
    this.ipc = new MyIpc(
        process.env.MY_IPC_SERVER_PORT || "40010",
        process.env.MY_IPC_CLIENT_PORT || "40110",
        this.ipcCallback   
    );

    this.ipcCallback = (i, d) =>{
        this.callback(i, d);        
    };

    this.addControl = (control) => {
        if (!this.controls.includes(control)){
            if (control.worker === undefined) control.worker = this;
            this.send(control.getCreationCommand());
            propsCommands = control.getPropertiesCommands()
            for(let i in propsCommands){
                this.send(propsCommands[i]);
            }
            this.controls[this.controls.length] = control;
        }
    };

    this.removeControl = (control) => {
        this.controls = this.controls.filter((c) => c != control);
        if(control.worker === self){
            control.worker = undefined;
            self.send(control.getDestructionCommand());
        }
    };

    this.print = (data)=>{
        this.send(`print '${data}';`);
    }

    this.send = (data) =>{
        try{
            this.ipc.send(data);
        }catch(e){
            console.log(data);
        }
    };

    this.listen = () =>{
        this.ipc.listen();
    };
};

exports.Control = function (controlType, name, properties = undefined, worker = undefined){
    this.name = name;
    this.controlType = controlType;
    this.properties = properties || {};
    this.worker = worker;
    if(this.worker !== undefined){
        this.worker.addControl(this);
    }

    this.setProperty = (name, value) =>{
        this.properties[name] = value;
        if(this.worker !== undefined){
            this.worker.send(`change-control '${this.name}' 'set-${name}=${value}';`);
        }
    };

    this.getCreationCommand = () =>{
        return `create-control ${this.controlType} '${this.name}';`;
    };

    this.getPropertiesCommands = () =>
    {
        return Object.keys(this.properties).map(
            (k)=>`change-control '${this.name}' 'set-${k}=${this.properties[k]}';`
        );
    };

    this.getDestructionCommand = () =>{
        return `remove-control  '${this.name}';`;
    };
}