# ORB (Opening Range Breakout) cAlgo Robot

## Description

Automated cAlgo robot designed to execute trades based on opening range breakouts, featuring optional martingale-style volume adjustment for risk management. This was made for a client who trades this as a manual strategy based on breakouts of high volume ranges created at New York Stock Exchange open times.

## Code Explanation

### Parameters

- **RangeStartTime:** Specifies the start time of the trading range.
- **ZoneColor:** Defines the color for visualizing breakout zones on the chart.
- **Volume:** Sets the initial trade volume.
- **StopLoss:** Determines the stop loss in pips.
- **TakeProfit:** Specifies the take profit in pips.
- **UseMultiplier:** Enables or disables the martingale-style volume adjustment.
- **Exponent:** Multiplier applied to volume if martingale is enabled.

### Initialization

- **OnStart():** Initializes the robot by converting `RangeStartTime` to a `TimeOnly` object and setting up initial trade parameters.
- **InitializeZones():** Initializes breakout zones based on `RangeStartTime` and current server time.

### Trading Logic

- **OnTick():** Called on each tick to draw breakout zones and potentially trigger trade entries.
- **OnBar():** Triggered on the formation of a new bar, resets zones and performs tests.
- **EnterTrade():** Executes market orders based on breakout conditions and manages trade execution.
- **Martingale():** Implements martingale logic to adjust trade volume based on previous trade outcomes if `UseMultiplier` is enabled.

### Utility Methods

- **ConvertToTime(string timeString):** Converts a string representation of time to a `TimeOnly` object.
- **FindHighLow(int startIndex, int endIndex):** Finds the highest high and lowest low within a specified bar range.
- **DrawZone():** Draws breakout zones on the chart during the specified trading range.
- **IsRangeTime():** Checks if the current server time is within the specified trading range.
- **IsTradingTime():** Determines if trading conditions are met based on breakout zone end time.

### ORZ Class

- **Properties:** Represents a breakout zone with start and end times, high, and low prices.

## Disclaimer
Trading in financial markets involves risk, and there is a possibility of losing capital. This bot should be used as a tool to assist in trading decisions and not as a guaranteed method for profit. Users are responsible for configuring and testing the bot to ensure it aligns with their trading strategy and risk tolerance. Past performance is not indicative of future results when backwards testing over historical data.
