# StreamerBot Integration with Warudo

![GitHub](https://img.shields.io/github/license/dbqt/WarudoStreamerBot)

This plugin aims to connect Warudo with StreamerBot.

[Install from Steam Workshop](https://steamcommunity.com/sharedfiles/filedetails/?id=3260939914)

## Features
- Node `Do StreamerBot Action` : Execute any action that is available on StreamerBot from Warudo and optionally provide any arguments as a dictionary of strings.

## Notes
To make sending arguments easier, you can optionally use the [StringPair & Dictionary plugin](https://steamcommunity.com/sharedfiles/filedetails/?id=3256621282).

## Usage
1. Make sure your StreamerBot instance is running and the main websocket server is running.
2. Add the StreamerBot asset to your scene and configure the IP Address and Port to match StreamerBot's websocket.
3. If the asset is not yet connected to StreamerBot, click "Refresh connection".
4. Add the `Do StreamerBot Action` node to a blueprint.
5. Pick an action from the list to trigger.
6. Setup the flow to trigger this node.

## Future plans
Eventually, I want to add support for receiving events from StreamerBot.

## Release notes
2024-06-04: Initial release with "Do StreamerBot Action" node.

## Support
If you need help, please open a [GitHub issue](https://github.com/dbqt/WarudoStreamerBot/issues) or ask on the [Discord](https://discord.com/invite/kmdh6RQ)

## License

This is under MIT license.