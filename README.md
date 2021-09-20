## Vk Auto Responder

This app will pool the provided VK chats and look for keywords.
When the match is found, it will reply to user with your message.

Feel free to open a PR or contact me directly for any info.

Binaries aren't provided

Usage (from source): 
- ```bash
    git clone --recursive https://github.com/Vk-Auto-Responder/vk-auto-responder.git
  ```
- Create a _secret_ folder.
- Create a _settings.json_ file inside it.
- Fill this file with your parameters like follows
    ```json
    {
        "auth-params": {
          "login": "string",
          "password": "string",
          "appId": 0
        },
        "chat-ids": [0],
        "keywords": ["string"],
        "banned-to-all-keywords": ["string"],
        "reply": "string"
    }
    ```
- Now you can build the app
- In your IDE set working directory to the location of _secret_ folder, or just put this folder near the executable. 
- Now you can run the app.