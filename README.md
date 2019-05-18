# Simple-Brute-Force
simple .netcore brute force created for an assessment.

+ Assuming the text file is generated before hand, the run time for cracking the password in this case (only ~1200 possible password combinations) was around 0.5 seconds.

The aims of this console application was:

+ To generate a text file containing all the permutations of the word 'password' with a few conditions like the letter a could 
be an @ and so forth.
+ To use said text file to attack a url with a known username 'admin' - if a sucessful combination was found a url is returned to you.
+ You must then use this url to send a zip file containing a few files.
