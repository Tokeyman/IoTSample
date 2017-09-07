# GLibrary.Device.GPIO Namespace

> General Purpose IO 



## GPIO Class

### Remarks

Typical order of operations

- Use GPIO(int PinNumber) or GPIO(int PinNumber,GpioSharingMode SharingMode) Initialize GPIO
- (Optional) set DebounceTimeOut if the pin will work under input mode.
- Set pin drive mode by methord SetDriveMode(GpioPinDriveMode PinDriveMode). Call GetDriveMode() returns actural PinDriveMode.
- Read() or Write(GpioPinValue Value) to read or set the voltage on pin.
- PinValueChanged event will raised if set.
- Dispose() will dispose the object.

### Properties

#### PinNumber

Get PinNumber currently use.

```c#
public int PinNumber{ get; private set;}
```

#### DebounceTimeout

Get or set debounce timeout 

```c#
public TimeSpan DebounceTimeout{ get; set; }
```

### Contructors

#### GPIO(int PinNumber)

Create a new Gpio Pin with PinNumber

```c#
public GPIO(int PinNumber)
```

#### GPIO(int PinNumber, GpioSharingMode SharingMode)

Create a new Gpio Pin with PinNumber and Set GpioSharingMode

```c#
public GPIO(int PinNumber, GpioSharingMode SharingMode)
```

### Methords

#### SetDriveMode(GpioPinDriveMode PinDriveMode)

Set PinDriveMode, such as Input, Output, Input with pull-up and so on.

```c#
public void SetDriveMode(GpioPinDriveMode PinDriveMode)
```

#### GetDriveMode()

Get PinDriveMode value.

```c#
public GpioPinDriveMode GetDriveMode()
```

#### Read()

Read pin value .

```c#
public GpioPinValue Read()
```

#### Write(GpioPinValue Value)

Write PinValue to pin.

```c#
public void Write(GpioPinValue Value)
```

#### Dispose()

Dispose the object

```C#
public override void Dispose()
```

### Events

#### PinValueChanged

An event that indicates that pin value has changed on the pin

```c#
public event TypedEventHandler<GPIO, GpioPinValueChangedEventArgs> PinValueChanged;
```





