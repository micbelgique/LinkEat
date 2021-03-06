# Introduction 
This repository is the LinkEat app home.\
It contains all the source code needed to maintain, fix or improve the LinkEat app.\
It is continuously deployed onto the Microsoft Innovation Center azure account.

# Getting Started
Services:
>[.NetCore (Entity Framework Core)](https://docs.microsoft.com/en-us/ef/core/)\
>[.NetCore (BotFramework)](https://docs.microsoft.com/en-us/azure/bot-service/dotnet/bot-builder-dotnet-core-authentication)\
>[LUIS](https://eu.luis.ai/)\
>[Twilio](https://www.twilio.com/docs/api?filter-product=sms)

# Build and Test
1. Install visual studio 2017 (Preview for BOT)
2. Add the following environment variables
	1. BotConnectionInfo__AppId
	2. BotConnectionInfo__AppPassword
	3. Twilio__AccountSID
	4. Twilio__AuthToken
	5. Twilio__Phone
	6. LUIS__AppId
	7. LUIS__SubscriptionKey
	8. Slack__Token
3. Build and run
4. Enjoy