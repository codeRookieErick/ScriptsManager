<?

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

