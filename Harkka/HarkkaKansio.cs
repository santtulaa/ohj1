using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

public class HarkkaKansio : PhysicsGame

{
    //PhysicsObject hammas;
    const double liikkumisnopeus = 500;
    const double RUUDUN_LEVEYS = 50;
    const double RUUDUN_KORKEUS = 50;
    //MediaPlayer.Play("taustamusiikki_music");
    Image taustakuva = LoadImage("taustaKuva");
    //private SoundEffect maaliAani = LoadSoundEffect("maali.wav");

    //MediaPlayer TaustaMusiikki = LoadSoundEffect("TaustaMusiikki");
    private List<Label> valikonKohdat = new List<Label>();
    private readonly Image pahkinanKuva = LoadImage ("Pahkina");
    private readonly Image suklaanKuva = LoadImage("Suklaati");
    Pahkina pahkina;
    public override void Begin()
    

    {
        SetWindowSize(800, 800);
        Gravity = new Vector(0, -1000);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Valikko();
    }

    void LuoKentta()
    {
        Camera.ZoomToAllObjects();
        Camera.ZoomToLevel();
        TileMap kentta = TileMap.FromLevelAsset("kentta");
        kentta.SetTileMethod('x', LuoSeina);
        kentta.SetTileMethod('y', LuoPahkina); //lisaa parempi kuva?
        kentta.SetTileMethod('m', LuoPelaaja);
        kentta.SetTileMethod('s', LuoPelaaja2);
        kentta.SetTileMethod('k', LuoKerattava, "Folio");
        kentta.SetTileMethod('p', LuoKerattava, "joku");
        Level.Background.Image = taustakuva;
        Level.Background.ScaleToLevelFull();

        MediaPlayer.Play("TaustaMusiikki");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.1;

        kentta.Execute(RUUDUN_LEVEYS, RUUDUN_KORKEUS);
        Level.CreateBorders();
    }


    void Valikko()
    {
        ClearAll(); 
        //List<Label> valikonKohdat = new List<Label>();
        Label aloita = new Label("Aloita uusi peli"); 
        aloita.Position = new Vector(0, 40);  
        valikonKohdat.Add(aloita);
        Label lopeta = new Label("Lopeta peli");
        lopeta.Position = new Vector(0, -40);
        valikonKohdat.Add(lopeta);
        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }
        Level.Background.Color = Color.White;
        Mouse.ListenOn(aloita, MouseButton.Left, ButtonState.Pressed, AloitaPeli, null);
        Mouse.ListenOn(lopeta, MouseButton.Left, ButtonState.Pressed, Exit, null);

    }


    public void AloitaPeli()
    {
        ClearAll();
        LuoKentta();
    }
    //public void AloitusValikko()
    //{
    //    MultiSelectWindow alkuValikko = new MultiSelectWindow("Tervetuloa pelaamaan!", "Aloita peli", "Ohjeet", "Poistu pelistä");
    //    alkuValikko.Color = Color.Gold;
    //    Add(alkuValikko);
    //    alkuValikko.AddItemHandler(0, AloitaPeli);
    //    alkuValikko.AddItemHandler(1, Ohjeet);
    //    alkuValikko.AddItemHandler(2, Exit);
    //}


    private void ValikossaLiikkuminen(AnalogState hiirenTila)
    {
        foreach (Label kohta in valikonKohdat)
        {
            if (Mouse.IsCursorOn(kohta)) kohta.TextColor = Color.Red;
            else kohta.TextColor = Color.Black;
        }
    }


    public void LuoPahkina(Vector paikka, double leveys, double korkeus)
    {
        pahkina = new Pahkina(leveys, korkeus, 1);
        pahkina.Position = paikka;
        pahkina.MakeStatic();
        //pahkina.Image = LoadImage("Pahkina");
        pahkina.Image = pahkinanKuva;
        Add(pahkina);
    }


    public void LuoSeina(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject suklaa = new PhysicsObject(leveys, korkeus);
        suklaa.Position = paikka;
        suklaa.Image = suklaanKuva;
        suklaa.Image = LoadImage("Suklaati");
        suklaa.MakeStatic();
        Add(suklaa);
        suklaa.IgnoresExplosions = true;
    }


    public void LuoKerattava(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject kerattava = new PhysicsObject(leveys, korkeus);
        kerattava.Position = paikka;
        kerattava.Tag = "kerattava";
        kerattava.MakeStatic();
        kerattava.Image = LoadImage(kuvanNimi);
        Add(kerattava);
    }


    public void LuoPelaaja(Vector paikka, double leveys, double korkeus)
    {


        Pahkina pelaaja = new Pahkina(leveys, korkeus, 3);
        pelaaja.Position = paikka;
        pelaaja.Shape = Shape.Circle;
        pelaaja.Color = Color.Yellow;
        Add(pelaaja);
        pelaaja.Restitution = 0.0;
        pelaaja.LinearDamping = 0.95;

        pelaaja.IgnoresExplosions = true;

        AddCollisionHandler(pelaaja, "kerattava", PelaajaTormasiKerattavaan);

        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, null, pelaaja, new Vector(-liikkumisnopeus, 0));
        Keyboard.Listen(Key.Left, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, null, pelaaja, new Vector(liikkumisnopeus, 0));
        Keyboard.Listen(Key.Right, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.Down, ButtonState.Down, Liikuta, null, pelaaja, new Vector(0, -liikkumisnopeus));
        Keyboard.Listen(Key.Down, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.Up, ButtonState.Down, Liikuta, null, pelaaja, new Vector(0, liikkumisnopeus));
        Keyboard.Listen(Key.Up, ButtonState.Released, Liikuta, null, pelaaja, Vector.Zero);
        Keyboard.Listen(Key.Space, ButtonState.Pressed, Tiputa, "tiputa pommi", pelaaja);

        //pelaaja.Destroyed mitä tapahtuu jos pelaaja kuolee?
    }


    public void LuoPelaaja2(Vector paikka, double leveys, double korkeus)
    {
        Pahkina pelaaja2 = new Pahkina(leveys, korkeus, 3);
        pelaaja2.Position = paikka;
        pelaaja2.Shape = Shape.Circle;
        pelaaja2.Color = Color.Red;
        pelaaja2.Restitution = 0;
        Add(pelaaja2);
        pelaaja2.IgnoresExplosions = true;

        AddCollisionHandler(pelaaja2, "kerattava", PelaajaTormasiKerattavaan);

        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, null, pelaaja2, new Vector(-liikkumisnopeus, 0));
        Keyboard.Listen(Key.A, ButtonState.Released, Liikuta, null, pelaaja2, Vector.Zero);
        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, null, pelaaja2, new Vector(liikkumisnopeus, 0));
        Keyboard.Listen(Key.D, ButtonState.Released, Liikuta, null, pelaaja2, Vector.Zero);
        Keyboard.Listen(Key.S, ButtonState.Down, Liikuta, null, pelaaja2, new Vector(0, -liikkumisnopeus));
        Keyboard.Listen(Key.S, ButtonState.Released, Liikuta, null, pelaaja2, Vector.Zero);
        Keyboard.Listen(Key.W, ButtonState.Down, Liikuta, null, pelaaja2, new Vector(0, liikkumisnopeus));
        Keyboard.Listen(Key.W, ButtonState.Released, Liikuta, null, pelaaja2, Vector.Zero);

        Keyboard.Listen(Key.Z, ButtonState.Pressed, Tiputa, "tiputa pommi", pelaaja2);
        //pelaaja2.Destroyed mitä tapahtuu jos pelaaja kaksi kuolee?
    }


    public void PelaajaTormasiKerattavaan(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        MessageDisplay.Add("Kerättiin jotakin");
        kohde.Destroy();
    }


    void Liikuta(PhysicsObject pelaaja, Vector suunta)
    {
        pelaaja.Velocity = suunta;
    }


    public void PommiTormasi(PhysicsObject pelaaja, Pahkina pahkina)
    {
        pahkina.OtaVastaanOsuma();
    }


    public void Tiputa(PhysicsObject heittavaOlio)
    {
        PhysicsObject pommi = new PhysicsObject(RUUDUN_LEVEYS, RUUDUN_KORKEUS);
        pommi.Shape = Shape.Circle;
        pommi.Color = Color.Beige;
        pommi.Position = heittavaOlio.Position;
        AddCollisionHandler<PhysicsObject, Pahkina>(pommi, PommiTormasi);
        pommi.MaximumLifetime = TimeSpan.FromSeconds(3.0);
        pommi.MakeStatic();
        Add(pommi);

        Timer.SingleShot(2.0, delegate
            {
                Explosion rajahdys = new Explosion(50);
                //rajahdys.UseShockWave = false; //rajahdyksen poisto

                rajahdys.Position = pommi.Position;
                rajahdys.Speed = 100;
                rajahdys.Force = 500;
                rajahdys.ShockwaveColor = Color.Yellow;
                rajahdys.ShockwaveColor = new Color(255, 0, 150, 90);
                Add(rajahdys, 2000);
                //rajahdys.AddShockwaveHandler(pahkina, PaineaaltoOsuu);
                //rajahdys.AddShockwaveHandler("Pahkina", PaineaaltoOsuu);
                rajahdys.ShockwaveReachesObject += PaineaaltoOsuu;
                

            }
        );
        void PaineaaltoOsuu(IPhysicsObject olio, Vector shokki)
        {
            pahkina.Destroy();
            pahkina.Destroy();

        }
        //Explosion rajahdys = new Explosion(2);
        ////rajahdys.UseShockWave = false; //rajahdyksen poisto

        //rajahdys.Position = pommi.Position; 
        //rajahdys.Speed = 100;
        //rajahdys.Force = 500;
        //rajahdys.ShockwaveColor = Color.Yellow;
        //rajahdys.ShockwaveColor = new Color(255, 0, 150, 90);
        //Add(rajahdys, 2);
        //pommi.Destroy();
        //Timer pommiajastin = new Timer();
        //pommiajastin.Interval = 2.0;
        ////pommiajastin.Timeout += delegate //Add(rajahdys);
        //pommiajastin.Start();
        //pommi.Destroyed += pommiajastin.Stop;
        //Add(rajahdys);
    }
    }