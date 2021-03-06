﻿using Gadgeteer.Modules.GHIElectronics;
using System;
using System.Collections;
using System.Threading;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation.Shapes;
using Microsoft.SPOT.Touch;
using Microsoft.SPOT.Input;
using System.IO;
using System.Net;
using System.Text;
using Gadgeteer.Networking;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Json.NETMF;

namespace BoardApplication
{
    public partial class Program
    {
        //private ConnectionManagement connection;
        //Variable used to display the GUI.
        private Client client;
        private Text txtMessage;
        private Bitmap normalButton;
        private Bitmap pressedButton;
        private Image imgButton;
        private Boolean flagButtonPressHere = false;
        private Window window;
        private Hashtable l = new Hashtable();
        private Hashtable deleteProducts = new Hashtable();
        private int flagThread = 0;
        private HttpWebRequest clientReq;
        private int WindowGlod = 0;
        private Boolean barcodeError = false;
        private GT.Picture picture;
        private UserInfo user;
        private Boolean globalAuth = false;
        private Boolean firstPicture = true;
        private String stringError = "Error: repeat the scanning of the picture";
        private Image deleteButton;
        private Bitmap bitmapNormalButton;
        private Bitmap bitmapPressedButton;
        private Boolean flagDeleteButton=false;
        private Boolean firstConnection = true;
        private Boolean connection = true;

        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {    
            // Use Debug.Print to show messages in Visual Studio's "Output" window during debugging.
             String[] array = {"192.168.1.1"};
             multicolorLED.TurnWhite();
             ethernetJ11D.NetworkInterface.Open();
             ethernetJ11D.NetworkInterface.EnableStaticIP("192.168.1.2", "255.255.255.0", "192.168.1.1");
             ethernetJ11D.NetworkInterface.EnableStaticDns(array);
             
             ethernetJ11D.NetworkUp += ethernetJ11D_NetworkUp;
             ethernetJ11D.NetworkDown += ethernetJ11D_NetworkDown;

             while (ethernetJ11D.NetworkInterface.IPAddress == "0.0.0.0")
             {
                 Debug.Print("Waiting for DHCP");
                 Thread.Sleep(250);
             }

            camera.PictureCaptured += new Camera.PictureCapturedEventHandler(camera_PictureCaptured);
            camera.TakePicture();
           // camera.TakePicture();
            button.ButtonPressed += new Button.ButtonEventHandler(button_ButtonPressed);
                          
            Debug.Print("Program Started");

            //welcome tune
           
            Tunes.MusicNote[] notes = new Tunes.MusicNote[4];
            notes[0] = new Tunes.MusicNote(Tunes.Tone.C4, 150);
            notes[1] = new Tunes.MusicNote(Tunes.Tone.E4, 150);
            notes[2] = new Tunes.MusicNote(Tunes.Tone.G4, 150);
            notes[3] = new Tunes.MusicNote(Tunes.Tone.C5, 300);

            tunes.Play(notes);

            window = displayTE35.WPFWindow;

            
            createWindowOne();
         
        }

      
        //Touch event linked to the first window.
        void imgButton_TouchUp(object sender, TouchEventArgs e)
        {
            imgButton.Bitmap = normalButton;
            if (flagButtonPressHere == true)
            {
                flagButtonPressHere = false;
                createWindowTwo();

            }
        }

        void imgButton_TouchDown(object sender, TouchEventArgs e)
        {
            imgButton.Bitmap = pressedButton;
            flagButtonPressHere = true;
        }

        Text addText(int dim1, int dim2, string text){
           
            Font baseFont = Resources.GetFont(Resources.FontResources.Calibri);
            txtMessage = new Text(baseFont, text);
            Canvas.SetTop(txtMessage, dim1);
            Canvas.SetLeft(txtMessage, dim2);
           
            return txtMessage;
        }

        //FIRST WINDOW
        void createWindowOne()
        {
            user = null;
            globalAuth = false;
            barcodeError = false;
            flagButtonPressHere = false;
            flagDeleteButton = false;
            byte[] normalButtonByte;
            byte[] pressedButtonByte;
            Canvas canvas = new Canvas();
            window.Child = canvas;
            window.Background = new SolidColorBrush(GT.Color.White);
            WindowGlod = 1;

            l.Clear();
            Font baseFont = Resources.GetFont(Resources.FontResources.Calibri);

            txtMessage = addText(30, 45, "Welcome to the automatic cash.");
            canvas.Children.Add(txtMessage);

            txtMessage = addText(50, 40, "Start your shopping pressing the");
            canvas.Children.Add(txtMessage);

            txtMessage = addText(70, 130, "button!");
            canvas.Children.Add(txtMessage);

            
            normalButtonByte = Resources.GetBytes(Resources.BinaryResources.NormalButton);
            pressedButtonByte = Resources.GetBytes(Resources.BinaryResources.PressedButton);
            normalButton = new Bitmap(normalButtonByte, Bitmap.BitmapImageType.Jpeg);
            pressedButton = new Bitmap(pressedButtonByte, Bitmap.BitmapImageType.Jpeg);

            //normalButton.SetPixel(154, 55, GT.Color.Blue);
            imgButton = new Image(normalButton);
            canvas.Children.Add(imgButton);
            Canvas.SetTop(imgButton, 110);
            Canvas.SetLeft(imgButton, 80);
            imgButton.TouchDown += new TouchEventHandler(imgButton_TouchDown);
            imgButton.TouchUp += new TouchEventHandler(imgButton_TouchUp);
        }

        //SECOND WINDOW
        void createWindowTwo()
        {
            WindowGlod = 2;
            Canvas canvas = new Canvas();
            window.Child = canvas;
            Font baseFont = Resources.GetFont(Resources.FontResources.Calibri);
                
            if (globalAuth == false)
            {
                txtMessage = addText(70, 75, "SCAN YOUR QR CODE ");
                canvas.Children.Add(txtMessage);

                txtMessage = addText(100, 108, "IN FRONT OF ");
                canvas.Children.Add(txtMessage);

                txtMessage = addText(130, 110, "THE CAMERA");
                canvas.Children.Add(txtMessage);
            }
            else
            {
                tunes.Play(new Tunes.MusicNote(Tunes.Tone.D3, 200));

                txtMessage = addText(30, 50, "The authentication is failed.");
                canvas.Children.Add(txtMessage);

                txtMessage = addText(75, 50, "Please scan again your QR code ");
                canvas.Children.Add(txtMessage);

                txtMessage = addText(95, 75, "in front of the camera.");
                canvas.Children.Add(txtMessage);
            }
                 
        }

        //Touch linked to the second window
        void imgButton_TouchUp2(object sender, TouchEventArgs e)
        {
            imgButton.Bitmap = normalButton;
            if (flagButtonPressHere == true)
            {
                flagButtonPressHere = false;
                createWindowThree();

            }
        }

        void imgButton_TouchDown2(object sender, TouchEventArgs e)
        {
            imgButton.Bitmap = pressedButton;
            flagButtonPressHere = true;
        }

        //WINDOW THREE
        void createWindowThree()
        {
            byte[] normalButtonByte;
            byte[] pressedButtonByte;
            byte[] deleteNormalButtonByte;
            byte[] deletePressedButtonByte;
           

            Canvas canvas = new Canvas();
            window.Child = canvas;
            Font baseFont = Resources.GetFont(Resources.FontResources.Calibri);

            txtMessage = addText(10, 30, "List of goods:");
            canvas.Children.Add(txtMessage);

            deleteButton = new Image();
            
            WindowGlod = 3;
          
            int top = 30;
            int left = 30;
            int i=0;
            double totalPrice = 0, totalPoints = 0;
            foreach (DictionaryEntry d in l)
            {

                deleteNormalButtonByte = Resources.GetBytes(Resources.BinaryResources.NormalDelete);
                deletePressedButtonByte = Resources.GetBytes(Resources.BinaryResources.PressedDelete);

                bitmapNormalButton = new Bitmap(deleteNormalButtonByte, Bitmap.BitmapImageType.Jpeg);
                bitmapPressedButton = new Bitmap(deletePressedButtonByte, Bitmap.BitmapImageType.Jpeg);
                //bitmapNormalButton.SetPixel(10, 10, GT.Color.Blue);

                //bitmapNormalButton.SetPixel(25, 27, GT.Color.Blue);
                deleteButton = new Image(bitmapNormalButton);

                deleteButton.TouchDown += new TouchEventHandler(imgButton_TouchDownDelete);
                deleteButton.TouchUp += new TouchEventHandler(imgButton_TouchUpDelete);
                if (deleteProducts.Contains(d.Key))
                    deleteProducts.Remove(d.Key);
                
                deleteProducts.Add(d.Key, deleteButton);
                
                ProductInfo p = d.Value as ProductInfo;
                if (p.productName.Length >= 20) {
                    p.productName = p.productName.Substring(0, 20);
                    p.productName += "...";
                }
                String s = p.productName + " " + p.price.ToString("f") + "$" + "  " + p.Qty;
                totalPrice += (p.price*p.Qty);
                totalPoints += (p.points*p.Qty);
                

                Canvas.SetTop(deleteButton, top);
                Canvas.SetLeft(deleteButton, 280);
                canvas.Children.Add(deleteButton);
                i++;

                txtMessage = addText(top, left, s);
                canvas.Children.Add(txtMessage);
                top += 25;
            }
            String stringPrice = "Total price: " + totalPrice.ToString("n2")+"$";

            txtMessage = addText(170, 180, stringPrice);
            canvas.Children.Add(txtMessage);

            String stringPoints = "Total points: " + totalPoints;
            txtMessage = addText(195, 180, stringPoints);
            canvas.Children.Add(txtMessage);

            if (barcodeError == true)
            {
                Tunes.MusicNote[] notes = new Tunes.MusicNote[1];
                notes[0] = new Tunes.MusicNote(Tunes.Tone.D3, 150);
                
                tunes.Play(notes);

                txtMessage = addText(top, left, stringError);
                canvas.Children.Add(txtMessage);

                top += 15;
            }
            

            normalButtonByte = Resources.GetBytes(Resources.BinaryResources.payButton);
            pressedButtonByte = Resources.GetBytes(Resources.BinaryResources.payButtonPressed);
            normalButton = new Bitmap(normalButtonByte, Bitmap.BitmapImageType.Jpeg);
            pressedButton = new Bitmap(pressedButtonByte, Bitmap.BitmapImageType.Jpeg);
            //normalButton.SetPixel(154, 55, GT.Color.Blue);
            imgButton = new Image(normalButton);
            Canvas.SetTop(imgButton, 170);
            Canvas.SetLeft(imgButton, 5);
            canvas.Children.Add(imgButton);

            imgButton.TouchDown += new TouchEventHandler(imgButton_TouchDown3);
            imgButton.TouchUp += new TouchEventHandler(imgButton_TouchUp3);

            //I send the list to the web service:
         /*   for (i = 0; i < l.Count; i++)
            {
                byte[] productBytes = Encoding.UTF8.GetBytes(l[i].ToString());
                client.sendBytes(productBytes);
            }            */
        }

        private void imgButton_TouchDownDelete(object sender, TouchEventArgs e)
        {
            
            foreach (DictionaryEntry i in deleteProducts)
            {
                Image img= i.Value as Image;
                Image imgSender = sender as Image;
                if (img.Equals(imgSender))
                {
                    img.Bitmap = bitmapPressedButton;
                    flagDeleteButton = true;
                    break;
                }
            }
            
        }

        private void imgButton_TouchUpDelete(object sender, TouchEventArgs e)
        {

            if (flagDeleteButton == true)
            {
                flagDeleteButton = false;
                foreach (DictionaryEntry d in deleteProducts)
                {
                    Image img = d.Value as Image;
                    Image imgSender = sender as Image;
                    if (img.Equals(imgSender))
                    {

                        ProductInfo s = l[d.Key] as ProductInfo;

                        if (s.Qty == 1)
                        {
                            l.Remove(d.Key);
                            deleteProducts.Remove(d.Key);
                        }
                        else if(s.Qty > 1)
                        {
                            s.Qty--;
                            l[d.Key] = s;
                        }

                        createWindowThree();
                    }
                }
            }
            
        }

        //Touch linked to window three
        void imgButton_TouchUp3(object sender, TouchEventArgs e)
        {
            imgButton.Bitmap = normalButton;
            if (flagButtonPressHere == true)
            {
                flagButtonPressHere = false;
                int i;
                ArrayList list = new ArrayList();
                foreach (DictionaryEntry d in l)
                {
                    ProductInfo p = d.Value as ProductInfo;
                    String productId = p.IDProduct;

                    Double qty = p.Qty;
                    Hashtable hashtable = new Hashtable();

                    hashtable.Add("ID", productId);
                    hashtable.Add("Qty", qty);
                    list.Add(hashtable);
                }

                Hashtable receiptTable = new Hashtable();
                receiptTable.Add("UserID", user.UserID);
                receiptTable.Add("List", list);
                string json = JsonSerializer.SerializeObject(receiptTable);
                int token = client.AskToken();
                byte[] productBytes = Encoding.UTF8.GetBytes(json);
                client.SendData(productBytes, token);

                if (Utils.BytesToString(client.ReceiveData(token)).Equals("OK"))
                {
                    l.Clear();
                    createWindowFour();
                }
                else flagButtonPressHere = false;
                //createWindowOne();
            }
        }

        void imgButton_TouchDown3(object sender, TouchEventArgs e)
        {
            imgButton.Bitmap = pressedButton;
            flagButtonPressHere = true;
        }

        void createWindowFour()
        {
            WindowGlod = 4;

            Tunes.MusicNote[] notes = new Tunes.MusicNote[7];
            notes[0] = new Tunes.MusicNote(Tunes.Tone.C5, 50);
            notes[1] = new Tunes.MusicNote(Tunes.Tone.Rest, 25);
            notes[2] = new Tunes.MusicNote(Tunes.Tone.C5, 50);
            notes[3] = new Tunes.MusicNote(Tunes.Tone.Rest, 25);
            notes[4] = new Tunes.MusicNote(Tunes.Tone.C5, 50);
            notes[5] = new Tunes.MusicNote(Tunes.Tone.Rest, 25);
            notes[6] = new Tunes.MusicNote(Tunes.Tone.C5, 200);
            
            tunes.Play(notes);


            GT.Timer timer = new GT.Timer(3000); // Create a timer
            Canvas canvas = new Canvas();
            window.Child = canvas;
            Font baseFont = Resources.GetFont(Resources.FontResources.Calibri);

            txtMessage = addText(100, 50, "THANKS FOR YOUR PURCHASE!");
            canvas.Children.Add(txtMessage);

            timer.Tick += timer_Tick; // Run the method timer_tick when the timer ticks
            timer.Start(); // Start the timer
        }
       

        void timer_Tick(GT.Timer timer){
            timer.Stop();
            createWindowOne();
        }

        private void camera_PictureCaptured(Camera sender,GT.Picture foto)
        {
            
            picture = foto;
            if (firstPicture == false)
            {
                Thread t = new Thread(configurePicture);

                t.Start();
                t.Join();

        

                if (WindowGlod == 3)
                    createWindowThree();
                else
                {
                    
                    if ( user==null || !user.Type.Equals("user"))
                    {
                        globalAuth = true;
                        createWindowTwo();
                    }
                    else
                        createWindowPurchase();
                }

            }
            else firstPicture = false;
        }

        private void createWindowPurchase()
        {
            WindowGlod = 5;
            byte[] normalButtonByte;
            byte[] pressedButtonByte;
            Canvas canvas = new Canvas();
            window.Child = canvas;
            window.Background = new SolidColorBrush(GT.Color.White);
            
            Font baseFont = Resources.GetFont(Resources.FontResources.Calibri);

            txtMessage = addText(80, 100, "Welcome " + user.name);
            canvas.Children.Add(txtMessage);
            
            normalButtonByte = Resources.GetBytes(Resources.BinaryResources.startBuying);
            pressedButtonByte = Resources.GetBytes(Resources.BinaryResources.PressedStartBuying);
            normalButton = new Bitmap(normalButtonByte, Bitmap.BitmapImageType.Jpeg);
            pressedButton = new Bitmap(pressedButtonByte, Bitmap.BitmapImageType.Jpeg);
            normalButton.SetPixel(154, 55, GT.Color.Blue);
            imgButton = new Image(normalButton);
            Canvas.SetTop(imgButton, 110);
            Canvas.SetLeft(imgButton, 80);
            canvas.Children.Add(imgButton);
           // WindowGlod = 2;
            imgButton.TouchDown += new TouchEventHandler(imgButton_TouchDown2);
            imgButton.TouchUp += new TouchEventHandler(imgButton_TouchUp2);
        }

        object createWindowError(object foo)
        {
            Canvas canvas = new Canvas();
            window.Child = canvas;
            window.Background = new SolidColorBrush(GT.Color.Red);
            Font baseFont = Resources.GetFont(Resources.FontResources.Calibri);
            
            txtMessage = addText(100, 50, "THE CONNECTION IS DOWN!");
            canvas.Children.Add(txtMessage);

            return null;
        }

        private void configurePicture()
        {
            byte[] result = new byte[65536];
            int read = 0;
            result = picture.PictureData;
            Boolean wrongSituation = false;

            Boolean receivedToken = false;
            int token = 0;
            byte[] receivedMessage = null;
            while (receivedToken == false)
            {
                try
                {
                    token = client.AskToken();
                    client.SendData(result, token);
                    receivedMessage = client.ReceiveData(token);
                    receivedToken = true;
                }
                catch (Exception e)
                {
                    if (!ethernetJ11D.IsNetworkUp)
                    {

                        //tunes.Play(new Tunes.MusicNote(Tunes.Tone.A3, 200));
                        break;
                        //Dispatcher.CurrentDispatcher.BeginInvoke(createWindowError, null);
                        //Thread.Sleep(1000);
                    }
                }
            }
            if (receivedMessage == null)
            {
                barcodeError = true;
                return;
            }
            
                        
            if (barcodeError == true)
            {
             //   l.Remove("Error");
                barcodeError = false;
            }
            if (Utils.BytesToString(receivedMessage).Equals("Error"))
            {
                barcodeError = true;
                
                if (WindowGlod == 2)
                    globalAuth = true;
            }
            else
            {
                Hashtable hashTable = JsonSerializer.DeserializeString(Utils.BytesToString(receivedMessage)) as Hashtable;

                if (WindowGlod == 2)
                {
                    user = new UserInfo();
                    user.Type = hashTable["Type"] as String;
                    if (user.Type.Equals("user"))
                    {
                        Tunes.MusicNote[] notes = new Tunes.MusicNote[4];
                        notes[0] = new Tunes.MusicNote(Tunes.Tone.C5, 50);
                        notes[1] = new Tunes.MusicNote(Tunes.Tone.C4, 50);
                        notes[2] = new Tunes.MusicNote(Tunes.Tone.E4, 50);
                        notes[3] = new Tunes.MusicNote(Tunes.Tone.G4, 100);
                        tunes.Play(notes);
                        
                        user.name = hashTable["Name"] as String;
                        user.surname = hashTable["Surname"] as String;
                        user.UserID = hashTable["ID"] as String;
                    } else
                    {
                        user = null;
                    }
                }
                else
                {
                    ProductInfo prod = new ProductInfo();

                    prod.Type = hashTable["Type"] as String;

                    if (!prod.Type.Equals("product"))
                    {
                        barcodeError = true;
                    }
                    else {
                        Tunes.MusicNote[] notes = new Tunes.MusicNote[4];
                        notes[0] = new Tunes.MusicNote(Tunes.Tone.C5, 50);
                        notes[1] = new Tunes.MusicNote(Tunes.Tone.C4, 50);
                        notes[2] = new Tunes.MusicNote(Tunes.Tone.E4, 50);
                        notes[3] = new Tunes.MusicNote(Tunes.Tone.G4, 100);
                        tunes.Play(notes);

                        prod.IDProduct = hashTable["ID"] as String;
                        if (!l.Contains(prod.IDProduct))
                        {
                            prod.productName = hashTable["Product_name"] as String;
                            String priceString = hashTable["Price"] as String;
                            prod.price = Double.Parse(priceString);
                            String pointString = hashTable["Points"] as String;
                            prod.points = Double.Parse(pointString);
                            prod.Qty++;
                            l.Add(prod.IDProduct, prod);
                        }
                        else
                        {
                            (l[prod.IDProduct] as ProductInfo).Qty++;
                        }
                   
                    }
                    
                }
            }
        }

        private void button_ButtonPressed(Button sender, Button.ButtonState state)
        {

            if (WindowGlod == 3 || WindowGlod == 2)
            {

                if (camera.CameraReady)
                {
                    camera.TakePicture();
                    tunes.Play(1100, 300);
                }

            }
        }

        void ethernetJ11D_NetworkDown(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            createWindowError(null);
        }

        void ethernetJ11D_NetworkUp(GTM.Module.NetworkModule sender, GTM.Module.NetworkModule.NetworkState state)
        {
            Debug.Print("Network is up!");
         //   Debug.Print("My IP is: " + ethernetJ11D.NetworkSettings.IPAddress);
            IPEndPoint IPaddress = new IPEndPoint(IPAddress.Parse("192.168.1.1"), 8000);
            client = new Client(IPaddress);
            window.Background = new SolidColorBrush(GT.Color.White);

            switch (WindowGlod)
            {
                case 1:
                    createWindowOne();
                break;
                case 2:
                    createWindowTwo();
                break;
                case 3:
                    createWindowThree();
                break;
                case 4:
                    createWindowFour();
                break;
                case 5:
                    createWindowPurchase();
                break;
                default:
                    break;
            }

        }

    }
}