# Darkroom editor

Image handling made easy.

Client-side. Server-side.

## Working with negatives

### How-to load an image:

```csharp
  // simple load
  var img = new Negative("sample.jpg");
  
  // load and resize -- if only height or width is specified, the image is scaled proportionally
  var img = new Negative("sample.jpg", 1280, 720);
  
  // load from Uri -- resize applies here as well
  var img = new Negative("http://host.domain/imageName.jpg");
```

### Crop an image:

```csharp
  var img = new Negative("sample.jpg", 1280, 720);
  
  // this results in removing the right side of the image, resulting in a square image
  img.Cut(0, 0, 720, 720);
```

### Digitize an image:

```csharp
  var img = new Negative("sample.jpg");
  
  File.WriteAllText("dataUri.base64", img.Digitize().ToString());
  
  // Output: data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAAAAEA...
```

### Pixel-wise comparison:

```csharp
  var img = new Negative("sample.jpg"),
      dupeImg = new Negative("sample.jpg");
  
  if(img == dupeImg)
  {
    // DO-STUFF
  }
```

## Enter the darkroom with your negative
```csharp
  static void Main(string[] args)
        {
            Stopwatch cropTimer = Stopwatch.StartNew();

            Negative img = new Negative("sample.jpg", 1280, 720)
                                       .Cut(560, 0, 720, 720);
            cropTimer.Stop();
            Console.WriteLine("Resized and cropped: {0} seconds", cropTimer.Elapsed.TotalSeconds);

            Stopwatch processingTimer = Stopwatch.StartNew();

            using (Darkroom editor = new Darkroom(img))
            {
                editor
                    .BlackAndWhite(BlackAndWhiteMode.Regular)
                    .Invert()
                    .Contrast(50)
                    .Brightness(10)
                    .Saturation(-50)
                    .Vibrance(-50)
                    .Gammma(-50)
                    .Noise(25)
                    .Sepia()
                    .Hue(45)
                    .Tint("#efefef")
                    // when the negative gets washed, all of the called filters get applied
                    .Wash()
                    .Develop(string.Format(@"{0}.jpg", Environment.TickCount));
            }

            processingTimer.Stop();

            Console.WriteLine("All image filters applied: {0} seconds", processingTimer.Elapsed.TotalSeconds);
            Console.ReadLine();
        }
```
