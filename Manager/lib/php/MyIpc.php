<?PHP

function send($port, $data = "hello")
{
    $s = socket_create(AF_INET, SOCK_STREAM, SOL_TCP);
    if($s === false){
        print('ALV');
    }else{
        $r = socket_connect($s, "127.0.0.1", $port);
        if($r === false){
            print('ALV');
        }else{
            socket_write($s, $data, strlen($data));
            socket_close($s);
        }
    }
}


function create_control($port, $type, $name){
    send($port, "create-control '$type' '$name';");
}

function alter_control($port, $name, $property, $value){
    send($port, "change-control '$name' 'set-$property=$value';");
}

?>