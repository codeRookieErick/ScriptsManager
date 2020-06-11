<?php
class MyIpc{

    var $port;
    function __construct($port){
        $this->port = $port;
    }

    function send($data)
    {
        $s = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
        if($s === false){
            print("Can't create port $this->port");
        }else{
            $r = socket_connect($s, "127.0.0.1", $this->port);
            if($r === false){
                print("Can't connect port $this->port");
            }else{
                socket_write($s, $data, strlen($data));
                socket_close($s);
            }
        }
    }

    function set_control($name, $type, $attributes){
        $data = "create-control '$type' '$name';";
        foreach($attributes as $property => $value){
            $data.="change-control '$name' 'set-$property=$value';";
        }
        $this->send($data);
        print($data);
    }
}
?>