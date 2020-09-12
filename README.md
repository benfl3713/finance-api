# Finance API
Personal Finance Manager API

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
- A personal finance manager that uses this api
- A different database technology (The current design is based on interfaces which will allow different databases to be used)
