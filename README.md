# ProjectWeb
C# like php interface

Create a wwwroot folder in the working directory of the application.

Default document is: index.html

I currently use .html to process the documents.
You can write HTML outside the <?cs ?> tags or write html using Echo(string);
include and include_once are partially supported - use literal strings - doesn't support include_once x;

supports Echo - there are no overloads so to Echo you need to cast all values to string
supports Die - you can pass an Exception or Text - this will stop code from running.
if an exception is thrown the file is not cached.
you can access the Context for the http request using the global variable Context which is of type HttpListenerContext (I will be adding the php functions too)

https://docs.microsoft.com/en-us/dotnet/api/system.net.httplistenercontext?view=netframework-4.7.2

Below is an example of how close the code can be.

# php code:

https://www.w3schools.com/php/php_mysql_select.asp

```php
<?php
$servername = "localhost";
$username = "username";
$password = "password";
$dbname = "myDB";

// Create connection
$conn = new mysqli($servername, $username, $password, $dbname);
// Check connection
if ($conn->connect_error) {
    die("Connection failed: " . $conn->connect_error);
} 

$sql = "SELECT id, firstname, lastname FROM MyGuests";
$result = $conn->query($sql);

if ($result->num_rows > 0) {
    echo "<table><tr><th>ID</th><th>Name</th></tr>";
    // output data of each row
    while($row = $result->fetch_assoc()) {
        echo "<tr><td>".$row["id"]."</td><td>".$row["firstname"]." ".$row["lastname"]."</td></tr>";
    }
    echo "</table>";
} else {
    echo "0 results";
}
$conn->close();
?>
```

# CSharp code:

```csharp
<?cs
var servername = "localhost";
var username = "username";
var password = "password";
var dbname = "myDB";

var conn = new Mysql(servername, username, password, dbname);

if (!conn) {
    Echo("Connection failed: " + conn.Error());
	return;
}

var results = conn.Query("SELECT id, firstname, lastname FROM MyGuests");

if (results.NumberRows > 0) {
    Echo("<table><tr><th>ID</th><th>Name</th></tr>");
    // output data of each row
	  MysqlRow row;
    while(row = results.FetchAssoc()) {
        Echo("<tr><td>" + row["id"] + "</td><td>" + row["firstname"] + " " + row["lastname"] + "</td></tr>");
    }
    Echo("</table>");
} else {
    Echo("0 results");
}
conn.Close();
?>
```
