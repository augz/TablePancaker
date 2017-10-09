# TablePancaker
## Description
C# Class to Dynamically compress an unknown table dimension to a JSON array object representing the table for display on front end MV* Frameworks.

## Authors
 Stephen Simon, Kevin Djordjevic

## Version
 0.0.0.5 
 
## About
 I noticed in my industry there was a lot of struggle for a reliable 
 way to fetch data out of a SQL Database. Every project had its own method, 
 everyone power houses things into models, the end result is a jQuery or
 Angular output that just seemed to lack a consistancy. 
 On numerous occasions I discovered neglect to even close connections. It can 
 get pretty bad. So I decided with my birthday I'd right a nice little Open 
 source project to help out with that struggle that may exist elsewhere.
 The concept for this TablePancaker is to handle a table as it is displayed 
 on the web front end, which is a string. Numbers can be converted if 
 calculations are desired, but the main focus of this controller is to allow
 the web server code to be load balanced behind some kind of load balancer. 
 In the instance where we can take advantage of some web server caching too.
 This also helps offload some strain on some maybe less than averagely load 
 balanced database infrastructures. 
 