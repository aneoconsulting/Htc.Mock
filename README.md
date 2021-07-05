# Htc.Mock

This project is intended to build tests of different htc grid middlewares.

## How to build a Htc.Mock client ?

To build a client, one has to implement two interfaces with the SDK provided
by the htc grid middleware:
* Htc.Mock.IDataClient
* Htc.Mock.IGridClient

Then, one can build a Htc.Mock.Client instance and call its Start method to 
begin a computation.

## How to build a Htc.Mock worker library ?

1. Chose a IRequestRunner amongst the three proposed
2. Implement the dependencies required by the constructor
3. Integrate the Htc.Mock.GridWorker with the worker system provided by htc grid middleware

 