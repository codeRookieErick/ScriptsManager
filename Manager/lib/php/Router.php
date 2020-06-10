<?PHP

function ok($val){
    http_response_code(200);
    print($val);
}

function fail($str){
    http_response_code(400);
    //die();
}

function not_found(){
    http_response_code(404);
    print("Notto foundo");
}

function download($filePath, $fileName="download"){
    header('Content-Disposition: attachment; filename="'.$fileName.'"');
    readfile($filePath);
}

class Router{
    var $routes = array();
    var $onRequest = null;
    var $requiredParameters = null;

    function __construct(){
        //print('alv');
    }

    function __call($method, $args){
        if(count($args) >= 2){
            $this->routes[$args[0]] = array(
                "METHOD" => $method,
                "CALLBACK" => $args[1],
                "REQUIRED" => count($args) >= 3? $args[2] : array()
                );
        }
    }

    function on_request($callback){
        $this->onRequest = $callback;
    }

    function __destruct(){
        $success = true;
        $requestUri = $_SERVER['REQUEST_URI'];
        while(strpos($requestUri, '?') > 0){
            $requestUri = substr($requestUri, 0, strpos($requestUri, '?'));
        }
        while(substr($requestUri, strlen($requestUri)-1, 1) == "/"){
            $requestUri = substr($requestUri, 0, strlen($requestUri)-1);
        }

        if($requestUri == ''){
            $this->print_api();
            return;   
        }
        else if(!array_key_exists($requestUri, $this->routes)){
            not_found();
        }else{
            foreach($this->routes as $key => $value){
                if($requestUri == $key){
                    $params = array();
                    if($value["METHOD"] == "get"){
                        $params = $_GET;
                    }else if($value["METHOD"] == "post"){
                        $params = $_POST;
                    } 
                    foreach($value["REQUIRED"] as $param => $type){
                        if(array_key_exists($param, $params)){
                            $params[$param] = $this->parse($params[$param], $type);
                        }else{
                            fail("'$param' is required!");
                            $success = false;
                        }
                    }
                    if($success)$value["CALLBACK"]($params);
                    if($this->onRequest){
                        ($this->onRequest)($requestUri, $params);
                    }
                }
            }
        }
        
        //print(__FUNCTION__);
    }

    function print_api(){
        print('<!Doctype html><html><head><link rel="stylesheet" href="style.css"/></head><body>');
        $variables = array(
            "SERVER_PROTOCOL",
            "SERVER_NAME",
            "DOCUMENT_ROOT",
            "SCRIPT_NAME",
            "SERVER_SOFTWARE"
        );
        $format = '<strong>%s</strong>:%s<hr/>';
        foreach($variables as $v){
            if(array_key_exists($v, $_SERVER)){
                printf($format, $v, $_SERVER[$v]);
            }
        }

        foreach($this->routes as $route => $definition){
            echo '<div class="outset"><div class="apiDescriptor">';
            printf(
                '<div class="header"><h3><strong style="color:red;">[%s]:</strong> %s</h3></div>',
                $definition["METHOD"],
                $route
            );
            printf('<form class="content" method="%s" action="%s"><table>', $definition["METHOD"], $route);
            if(count($definition["REQUIRED"]) > 0){
                foreach($definition["REQUIRED"] as $param => $value){
                    $inputType = "text";
                    switch($value){
                        case "int":
                            $inputType = "number";
                            break;
                        default:
                            break;
                    }
                    printf(
                    '<tr><td><label style="padding:1em; font-weight:bold;">%s</label></td><td><input name="%s" type="%s"/></td></tr>',
                    $param,
                    $param,
                    $inputType
                );
                }
            }
                print(
                    '<tr><td><input name="submit" value="send" type="submit"/></td></td><td></tr>'
                );
                print("</table></form>");
                echo '</div></div>';
        }
        print("</body></html>");
    }

    function print_parameters($params){
        print(__FUNCTION__);
        foreach($params as $key => $value){
            print("$key ".gettype($this->parse($value)));
        }
    }

    function parse($val, $type){
        try{
            settype($val, $type);
            return $val;
        }catch(Exception $e){
            fail("CANT DO ANYTHING!");
        }
    }


}

?>