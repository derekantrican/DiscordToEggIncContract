# DiscordToEggIncContract

This program serves to watch for new coops posted to the Egg, Inc Discord (either the `#ads-only-standard` or 
`#ads-only-elite channels`) and report back what the highest roles are - ensuring you can pick the best coop.
It is against the ToS of Discord to automatically interact with a server without a bot being added to that
server, so this works by

1. Taking a screenshot of the Discord channel
2. Sending to ocr.space for text recognition
3. Parsing that text recognition for eicoop.netlify.app urls (thereby giving us the contract id & coop id)
4. Using that contract id & coop id in an API request to the wasmegg Egg, Inc API to check the 
   status of the coop
5. Reporting back to the console with that status (contract id, coop id, open spots, and highest role)
6. Repeat

## Features:

- Found coops will be stored and checked again every interval (so they will still be checked even if
  the url is no longer in the screenshot)
- Full or invalid coops (because sometimes OCR messes up) will be stored so they aren't checked again
- There is an option to filter by soul power (reference table below) so you will only see coops that
  have a highest role above a certain threshold
  
## Settings:

This program depends on a local (in the same directory) `settings.json` in order to run properly. Here is an annotated example:

```js
{
    "ocrSpaceApiKey" : "K12345678901234", //API key for OCR from ocr.space (get a free API key here: https://ocr.space/ocrapi/freekey)
    "intervalSeconds" : 30, //How long to wait in-between loops
    "soulPowerFilter" : 22, //Only print open coops with a highest role above this soul power (optional: set to -1 to not filter)
    "screenshotArea" : { //The location on your computer to take the screenshot (a screenshot tool like ShareX can help you get screen coordinates)
        "startX": 2870,
        "startY": 115,
        "endX" : 3570,
        "endY" : 1333
    }
}
```

## Demo:

![DiscordEggInc](https://user-images.githubusercontent.com/1558019/168669160-bd752ee5-0b87-4db3-9ab4-020c5a85773e.gif)
