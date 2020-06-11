<?PHP
	set_include_path(getenv("PHP_INCLUDE_PATH") ?? ".");
	require("ScriptsManager.php");

	///Enviar por aqui, (el otro, clientPort es para recibir cosas)
	$serverPort = getenv("MY_IPC_SERVER_PORT");
	$clientPort = getenv("MY_IPC_CLIENT_PORT");

	$ipc = new MyIpc($serverPort);
	$ipc->set_control("progress", "MyIpcProgressBar", array(
		"Title" => "Progress from php",
		"Maximun" => 9,
		"Value" => 3
	));
	
	print("Success!");
?>

