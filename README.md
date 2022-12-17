# MMORPGKIT DynamicClientServerConfig Addon
 A custom addon for the MMORPGKIT which uses StreamingAssets to store the server-connection details on client/player side in a CSV text file just like server does.
 Allows you to change the server list quickly and easily without needing to rebuild the client.
  
![Screenshot](https://i.imgur.com/LNzCPCh.jpeg)

 
## Advantages: ##
0. Uses StreamingAssets
1. Easier modification of server-connection-list details without rebuilding client, specially useful if need to switch server between dev/production
2. Manage multiple server configs and enable/disable which one to use by renaming it
3. The Server List is in CSV, to add more servers just add new entry on a new line
4. Super Helpful for use in CI/CD or need to deploy multiple versions of server/client but could not because with auto-deployment to multiple platforms, you would need to change server details for client 
Now, you can just add steps in your CD pipeline to rename or delete other configs
5. Can use hostname or IP
6. Supports all standalone platforms, ios, android and even WebGL
7. Falls back to whatever Server List you have setup in your Scene if cannot find the serverConfig CSV file or fails to read it

## How to use: ##
1. The addon is plug and play, just drop it in your project.
2. The addon automatically creates the `StreamingAssets` folder at root of your project's Assets Folder. This `StreamingAssets` folder contains example CSV file for serverList, please edit it to include your server list. 
This StreamingAssets folder is included with your builds automatically.
3. Edit `MMOClientInstance.cs` to change `MmoNetworkSetting[] networkSettings` from `private` to `internal scope`
4. Whatever serverList you set in the StreamingAssets folder in your project's Assets folder is included with your buidls (overwriting the build's previous serverList), but you can still edit and change the serverList from your build's StreamingAssets folder as required.

![image](https://user-images.githubusercontent.com/3790163/208266108-c1d87f1b-aae7-4990-930f-1d540153cd6d.png)
