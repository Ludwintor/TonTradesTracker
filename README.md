# Ton Trades Tracker
![Preview](https://github.com/Ludwintor/TonTradesTracker/raw/main/preview.png)

## Features
* Show actual current price in pool after trades
* Show price change between trades
* Only [DeDust](https://dedust.io) pools supported
* Only Direct swaps supported (do not show full routing but still show swap if this pool is involved in routing)

## Quick Start
1. Edit `Tracker` options in `TradesTracker/appsettings.json` for your token and pool.
Example (only hexadecimal address form supported):
```json
"Tracker": {
    "ChannelId": -1002070106680,
    "TokenAddress": "0:b113a994b5024a16719f69139328eb759596c38a25f59028b146fecdc3621dfe",
    "PoolAddress": "0:3e5ffca8ddfcf36c36c9ff46f31562aab51b9914845ad6c26cbde649d58a5588",
    "TradesPerPass": 10, // maximum trades per message
    "PassDelay": 10, // in seconds
    "ExplorerUrl": "https://tonviewer.com/"
}
```
2. Copy `example.env` as `.env` and add your telegram bot token (toncenter token usually not necessary as anonymous plan allows 1 RPS and this is sufficient for us)
```bash
cp example.env .env
```
```txt
BOT_TOKEN=
TONCENTER_TOKEN=
```
3. Run application in Docker Container
```bash
docker compose up -d
```

## Buy me a coffee

### TON (Toncoin, USDT)
`UQA705AUWErQe9Ur56CZz-v6N9J2uw298w-31ZCu475hT8U4`

### TRC20 (TRX, USDT)
`TEHvFyCMSQSGsKg1TVGCcCiDXr1DMs4MTe`

## FAQ

### How to get channel id?
Start bot [`@JsonDumpBot`](https://t.me/JsonDumpBot) and forward any message from target channel to this bot. Look for `forward_from_chat` property and find `id` in this object

### How to get DeDust pool address?
Open target pool at https://dedust.io/pools. Look at url again and copy address after `/pools/`

### How to convert address to hexadecimal form?
https://ton.org/address