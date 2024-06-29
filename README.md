# Ton Trades Tracker
![Preview](https://github.com/Ludwintor/TonTradesTracker/raw/main/preview.png)

## Features
* Show actual current price in pool after trades
* Show price change between trades
* Show daily, weekly and monthly price differences
* Only [DeDust](https://dedust.io) pools supported
* Only Direct swaps supported (do not show full routing but still show swap if this pool is involved in routing)

## Quick Start (Docker)
1. Edit `Tracker` options in `TradesTracker/config.json` for your jetton and pool (only hexadecimal address form supported). Add secrets for telegram bot and toncenter api key (you can ignore toncenter api key because without it we have 1 RPS and it's enough if you doesn't run multiple trackers)
Example (check FAQ below):
```json
"Tracker": {
    "ChannelId": -1002070106680,
    "TokenAddress": "0:b113a994b5024a16719f69139328eb759596c38a25f59028b146fecdc3621dfe",
    "PoolAddress": "0:3e5ffca8ddfcf36c36c9ff46f31562aab51b9914845ad6c26cbde649d58a5588",
    "TradesPerPass": 10, // maximum trades per message
    "PassDelay": 10, // in seconds
    "ExplorerUrl": "https://tonviewer.com/"
}
"BOT_TOKEN": "",
"TONCENTER_TOKEN": ""
```
2. Run application in Docker Container
```bash
docker compose -f docker-compose.yml up -d --build
```

## Quick Start (Local)
1. Install .NET SDK 8.0 or newer (if not installed)
```bash
dotnet --version
```
2. Build from source code
```bash
dotnet publish -c Release -o publish ./TradesTracker/TradesTracker.csproj
```
3. Go to `publish` directory and edit `config.json` as explained in step 1 of Docker start guide
4. Run application executable
   
Linux:
```bash
./TradesTracker
```
Windows:
```powershell
.\TradesTracker.exe
```

## Buy me a coffee

### TON (Toncoin, USDT)
```
UQA705AUWErQe9Ur56CZz-v6N9J2uw298w-31ZCu475hT8U4
```

### TRC20 (TRX, USDT)
```
TEHvFyCMSQSGsKg1TVGCcCiDXr1DMs4MTe
```

### ETH
```
0x95Ba8e4FeC184Ef983a89B020E6399Fa01E53bA3
```

### BTC
```
bc1q9czr3qmypd6xvt7m5c8lnnfh4e5ra6ppkjp78s
```

## FAQ

### How to get channel id?
Start bot [`@JsonDumpBot`](https://t.me/JsonDumpBot) and forward any message from target channel to this bot. Look for `forward_from_chat` property and find `id` in this object

### How to get DeDust pool address?
Open target pool at https://dedust.io/pools. Look at url again and copy address after `/pools/`

### How to convert address to hexadecimal form?
https://ton.org/address
