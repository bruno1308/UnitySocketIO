# Unity Socket IO

Unity Socket IO is a lightweigth library inspired by the great [Socket IO]. Note that it's not a port of Socket IO to Unity, but a fresh new library using the idea and nomenclature, built on top of a TCP Socket. **It's not ready yet for usage**, but I'll keep working on it.

### Version
This version **IS NOT** functional yet, I have just started working on it and I will provide the server side code soon (which is also not finished yet).

### Installation
Just import the files to your project and include the namespace 
```
using UnitySocket;
```

### Usage

* Initialize your Unity Socket:
```
Socket socket = IO.Socket("127.0.0.1", 9000, new Emitter.Listener(onConnect));
```
Where the parameters are the server ip, the server port, and a callback for handling when you are first connected.
* For Emitting commands to the server:
```
socket.Emit(OPCODE, data);
```
Where OPCODE is a number that will be recognized by your server as a custom action, and data is a array of bytes that your server will receive.
* For Listening to events sent by the server, do:
``` 
socket.On(OPCODE, new Emitter.Listener(onLogin));
```
Where OPCODE is a number the server will send that represents a type of packet that you will handle, and a callback function that will receive a array of bytes with the information sent by the server, like this:
```  
void onLogin(byte[] args)
    {
        Debug.Log("I am logged");
    }
```
### Development

Want to contribute? Great!

This project is a long way from being finished, anyone is welcome to help.

### Todos

 - Everything,
 - but mainly the server code
 
### Thanks
Thanks to [dillinger] that provided an awesome tool to format github READ.me, I had no idea how to do it.

License
----

MIT


**Free Software, Hell Yeah!**

[//]: # (These are reference links used in the body of this note and get stripped out when the markdown processor does its job. There is no need to format nicely because it shouldn't be seen. Thanks SO - http://stackoverflow.com/questions/4823468/store-comments-in-markdown-syntax)

   [Socket IO]: <http://socket.io>
   [dillinger]: <http://dillinger.io>
