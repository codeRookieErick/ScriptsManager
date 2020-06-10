from _thread import start_new_thread
from threading import Lock
import socket
import os
import time
import re

class MyIpc:    
    RECEIVE_BUFFER_LENGTH = 10240
    MAIN_THREAD_WAIT_INTERVAL = 1
    def __init__(self, serverPort, myPort, receiveCallback = None):
        if receiveCallback != None:
            self.receiveCallback = receiveCallback
        else:
            self.receiveCallback = None

        self.serverPort = serverPort
        self.port = myPort
        self.comunicationLock = Lock()
        self.running = False
        start_new_thread(self.start, ())

    def send(self, data:str):
        result = False
        try:
            self.comunicationLock.acquire()
            s = socket.socket()
            s.connect(('127.0.0.1', int(self.serverPort)))
            s.send(data.encode())
            s.close()
            result = True
        except Exception as e:
            print(e)
        finally:
            self.comunicationLock.release()
        return result

    def start(self):
        if self.running:
            return
        self.running = True
        try:
            s = socket.socket()
            s.bind(('', int(self.port)))
            s.listen(5)
            while self.running:
                c, addr = s.accept()
                if self.receiveCallback != None:
                    self.receiveCallback(self, c.recv(MyIpc.RECEIVE_BUFFER_LENGTH).decode())
                pass
        except Exception as e:
            print(e)
            pass

    def kill(self):
        self.running = False
    
class Control:
    def __init__(self, controlType, name, properties=None, worker=None):
        self.name = name
        self.controlType = controlType
        self.properties = properties or {}
        self.worker = worker
        if(self.worker != None):
            self.worker.addControl(self)

    def setProperty(self, name, value):
        self.properties[name] = value
        if(self.worker != None):
            self.worker.send("change-control '{0}' 'set-{1}={2}';".format(self.name, name, value))

    def getCreationCommand(self):
        return "create-control {0} '{1}';".format(self.controlType, self.name)

    def getPropertiesCommands(self):
        return ["change-control '{0}' 'set-{1}={2}';".format(self.name, i, self.properties[i]) for  i in self.properties]
            
    def getDestructionCommand(self):
        return "remove-control '{0}';".format(self.name)

class Worker:
    DEFAULT_SERVER_PORT = 40010
    DEFAULT_CLIENT_PORT = 40110
    def __init__(self, receiveCallback = None):
        self.receiveCallback = receiveCallback
        self.ipc = MyIpc(
            os.environ.get('MY_IPC_SERVER_PORT') or Worker.DEFAULT_SERVER_PORT,
            os.environ.get('MY_IPC_CLIENT_PORT') or Worker.DEFAULT_CLIENT_PORT,
            (lambda i, d: self.receive(i, d)) 
        )
        self.dataStack = []
        self.controls = []
        self.initCommands()
        

    def initCommands(self):
        self.commands = {
            "list-of-controls":lambda: self.list_of_controls()
        }

    def list_of_controls(self):
        pass

    def addControl(self, control):
        if control not in self.controls:
            if control.worker == None:
                control.worker = self
            self.send(control.getCreationCommand())
            for cmd in control.getPropertiesCommands():
                self.send(cmd)
            self.controls.append(control)

    def removeControl(self, control):
        if control in self.controls:
            self.controls.remove(control)
        if control.worker == self:
            control.worker = None
        self.send(control.getDestructionCommand())
            
    def receive(self, ipc, data):
        commands = []
        first = True
        for i in re.findall(r"'[^']+'|[\w\d\-\_]+", data):
            if i in self.commands:
                commands.append(self.commands[i])
            elif first:
                break
            else:
                self.dataStack.append(i)
            if first:
                first = False
        
        while len(commands) > 0:
            commands.pop()()
        self.dataStack = []
        if(self.receiveCallback != None):
            self.receiveCallback(self.ipc, data)

    def print(self, data):
        self.send("print '{0}'".format(data))

    def notify(self, text, title = None):
        self.send("notify '{0}' '{1}'".format(text, title or "Notification"))

    def send(self, data):
        try:
            self.ipc.send(data)
        except:
            print(data)

    def kill(self):
        for c in self.controls:
            self.removeControl(c)
        self.send('kill')
        self.ipc.kill()