# Finance API
![](https://github.com/benfl3713/finance-api/workflows/Pipeline/badge.svg?branch=master)  

**Personal Finance Manager API**

This api is used by this front end finance manager https://github.com/benfl3713/finance-manager

# Purpose
This api provides a backend for a personal finance manager.  Including a JWT authentication system for clients
It will allow you do CRUD operations on:
- Clients
- Accounts
- Transactions

Also You can
- Sync your bank account to the api so it will add the transactions for you (Using https://truelayer.com)
- Calculate transaction logos based off the transaction details

# Technology
- Asp.Net Core Web Api
- Mongodb database to store all data

# Future Features
- Statistic Calculations based of transaction data
- A different database technology (The current design is based on interfaces which will allow different databases to be used)
- Import different format files (Export from bank and then upload to this api)

# Need Help?

- If you have any questions feel free to raise an issue or email me at **benfl3713@gmail.com**

# Docker  
If you want to run the finance manager and finance api and database all together then you can use the following docker-compose configuration.
(This assumes you are running on windows. Just modify the volume mappings if you're using linux or mac os to a different host directory)

```yaml
version: "3"

services:
  finance-manager:
    image: benfl3713/finance-manager:latest
    depends_on:
      - finance-api
    ports:
      - "5005:80"
    environment:
      FinanceApiUrl: "http://localhost:5001/api"
  finance-api:
    image: benfl3713/finance-api:latest
    depends_on:
      - mongo-db
    ports:
      - "5001:80"
    volumes:
      - c:/finance-api:/app/config
    environment:
      MongoDB_ConnectionString: "mongodb://mongo-db"
  mongo-db:
    image: mongo
    ports:
      - "27017-27019:27017-27019"
    volumes:
      - c:/mongodb/finance:/data/db
```

