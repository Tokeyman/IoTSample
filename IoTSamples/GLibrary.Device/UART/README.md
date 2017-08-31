# GLibrary.Device.UART Namespace

> SerialPort 

## SerialPort Class

### Remarks

System-internal or on-chassis serial ports may be enumerated by [DeviceInformation.FindAllAsync()](https://docs.microsoft.com/en-us/uwp/api/windows.devices.enumeration.deviceinformation#Windows_Devices_Enumeration_DeviceInformation_FindAllAsync_System_String_), but cannot be opened by [SerialDevice.FromIdAsync()](https://docs.microsoft.com/en-us/uwp/api/windows.devices.serialcommunication.serialdevice#Windows_Devices_SerialCommunication_SerialDevice_FromIdAsync_System_String_)because they currently are not supported. However, serial ports connected over USB, such as on USB-to-Serial cables are supported.

### Properties

#### Selector

`string` port name such as `COM1`

```c#
public string Selector{ get; private set; }
```

#### BaudRate

Get SerialPort BaudRate

```c#
public int BaudRate{ get; private set; }
```



### Constructors

#### SerialPort(string Selector, int BaudRate)

Create a SerialPort with `Selector` and `BaudRate` 

```c#
public SerialPort(string Selector, int BaudRate)
```

### Methords

#### GetPortNames()

Get all avaliable ports.

```c#
public static async Task<List<string>> GetPortNames()
```



#### Open()

Open SerialPort

```c#
 public async void Open()
```



#### Send(byte[] Buffer)

Send a bytes buffer 

```c#
public async void Send(byte[] buffer)
```



#### Dispose()

Dispose SerialPort

```c#
public override void Dispose()
```





### Events

#### DataReceived

An event that indicates one or more bytes have received.

```c#
public event TypedEventHandler<SerialPort, SerialDataReceivedArgs> DataReceived;
```







## SerialDataReceivedArgs Class

> DataReceived event pass this class

### Properties

#### BytesToRead

Bytes have received

```c#
public int BytesToRead { get; set; }
```

#### ReceivedBuffer

Received message bytes buffer

```c#
public byte[] ReceivedBuffer { get; set; }
```

### Constructors

#### SerialDataReceivedArgs(int BytesToRead, byte[] ReceivedBuffer)

Create a SerialDataReceivedArgs

```c#
 public SerialDataReceivedArgs(int BytesToRead, byte[] ReceivedBuffer)
```





