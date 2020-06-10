<?PHP
	set_include_path(getenv("PHP_INCLUDE_PATH"));
	require("Router.php");
	require("MyIpc.php");

	///Enviar por aqui, (el otro, clientPort es para recibir cosas)
	$serverPort = getenv("MY_IPC_SERVER_PORT");

	
	$clientPort = getenv("MY_IPC_CLIENT_PORT");
	
	function query_as_array($query, $database = 'database.db'){
		$data = (new SQLite3($database))->query($query);
		$result = array();
		while($line = $data->fetchArray()){
			$l = array();
			$n = 0;
			foreach($line as $key => $value){
				$n+=1;
				if($n%2==0){
					$l[$key] = $value;
				}
			}
			array_push($result, $l);
		}
		return $result;
	}
	
	function query_as_json($query, $database = 'database.db'){
		return json_encode(query_as_array($query, $database)??array());
	}

	$app = new Router();	

	$app->on_request(function($url, $params){
		global $clientPort, $serverPort;
		//print($clientPort);
		//print_r($params);

		//$port = print(getenv("MY_IPC_SERVER_PORT"));
		//print($port."<br/>");

		//create_control($port, "MyIpcLabel", "label");
		//alter_control($port, "label", "Text", "Superalaverg");
	});

	$app->get("/getImage", function($params){
		download("jpg.jpg");
	}, array("token" => "string"));

	$app->get("/getCurrency", function($params){
		print(query_as_json("select * from vw_currency;"));
	}, array("token" => "string"));

	$app->post("/getQuotes", function($params){
		readfile("quotes.json");
	});

	//$-S 127.0.0.1:80 -t ${script}
?>

