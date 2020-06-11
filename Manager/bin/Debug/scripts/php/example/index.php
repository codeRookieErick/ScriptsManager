<?PHP
	set_include_path(getenv("PHP_INCLUDE_PATH"));
	require("MyIpc.php");

	///Enviar por aqui, (el otro, clientPort es para recibir cosas)
	$serverPort = getenv("MY_IPC_SERVER_PORT");
	$clientPort = getenv("MY_IPC_CLIENT_PORT");
	
	create_control($serverPort, "MyIpcLabel", "label");
	alter_control($serverPort, "label", "Text", "Superalaverg");
	print("Success");

?>

