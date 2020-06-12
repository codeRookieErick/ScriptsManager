<?php

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