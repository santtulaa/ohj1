using System;
using System.Collections.Generic;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

/// @author Santeri Salmela
/// version 23.4.2021
///TODO: Funktio/Silmukka ks, https://tim.jyu.fi/view/kurssit/tie/ohj1/2021k/demot/demo7#7qEWCnqCD75y

public class FarseerHarkka : PhysicsGame
{
    /// <summary>
    /// Voima jolla pelaajat liikkuvat
    /// </summary>
    const double LIIKUTUSVOIMA = 200;

    /// <summary>
    /// Voima jolla pelaajat hyppäävät
    /// </summary>
    const double HYPPYVOIMA = 500;

    /// <summary>
    /// Määritellään olioiden leveys
    /// </summary>
    const double RUUDUN_LEVEYS = 50;

    /// <summary>
    /// Määritellään olioiden korkeus
    /// </summary>
    const double RUUDUN_KORKEUS = 50;

    /// <summary>
    /// Määritellään painovoima, joka vaikuttaa olioihin
    /// </summary>
    private const double PAINOVOIMA = -1000;

    /// <summary>
    /// Määritellään olioille kuvat
    /// </summary>
    private readonly Image pahkinanKuva = LoadImage("rikkinainen");
    private readonly Image suklaanKuva = LoadImage("Suklaati");
    private readonly Image maalinKuva = LoadImage("maaliKuva");
    private readonly Image portaatKuva = LoadImage("valkonen");
    private readonly Image piikkilankaKuva = LoadImage("piikkilanka");
    private readonly Image haamuseinaKuva = LoadImage("lapinakyva");

    /// <summary>
    /// Määritellään keltaiselle animaatio
    /// </summary>
    private Image[] keltaisenAnimaatio = LoadImages("salama", "salama2", "salama3", "salama4");

    /// <summary>
    /// Määritellään siniselle animaatio
    /// </summary>
    private Image[] sinisenAnimaatio = LoadImages("aallot", "aallot2", "aallot3", "aallot4", "aallot5", "aallot6", "aallot5", "aallot4", "aallot3", "aallot2");

    /// <summary>
    /// Määritellään kentän taustakuva
    /// </summary>
    Image taustakuva = LoadImage("tausta");

    /// <summary>
    /// Määritellään pelaajille hyppyääni
    /// </summary>
    SoundEffect hyppyAani = LoadSoundEffect("hop");

    /// <summary>
    /// Määritelläään valikonkohdat valikkoon
    /// </summary>
    List<Label> valikonKohdat;

    /// <summary>
    /// Luodaan pahkina omalle sivullen
    /// </summary>
    Pahkina pahkina;

    /// <summary>
    /// Portaita käytetään, kun törmätään vipuun
    /// </summary>
    private PhysicsObject portaat;

    /// <summary>
    /// Pelaajan pitää pystyä törmäämään kaikkiin
    /// </summary>
    private PlatformCharacter pelaaja;

    /// <summary>
    /// Toisen pelaajan pitää pystyä törmäämään kaikkiin
    /// </summary>
    private PlatformCharacter pelaaja2;

    /// <summary>
    /// Määritellään kenttänumeron alkavan ykkösestä
    /// </summary>
    int kenttaNro = 1;


    /// <summary>
    /// Pääohjelma jossa näytetään alkuvalikko, josta voidaan lähteä liikkelle
    /// </summary>
    public override void Begin()
    {
        AlkuValikko();
    }


    /// <summary>
    /// Aliohjelma luo valikon
    /// </summary>
    private void AlkuValikko()
    {
        MultiSelectWindow alkuValikko = new MultiSelectWindow("Pelin alkuvalikko",
        "Aloita peli", "Lopeta");
        Add(alkuValikko);
        Mouse.IsCursorVisible = true;
        IsFullScreen = true;
        MediaPlayer.Play("TaustaMusiikki");
        MediaPlayer.IsRepeating = true;
        MediaPlayer.Volume = 0.2;

        Valikko("", "Aloita uusi peli");
        Level.Background.Image = LoadImage("tausta");

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// luo kentän kenttatiedostonnimen mukaan.
    /// </summary>
    /// <param name="kenttatiedostonNimi">kenttä mikä luodaan</param>
    public void LuoKentta(string kenttatiedostonNimi)
    {
        Level.Background.Image = LoadImage("tausta");
        Level.Background.ScaleToLevelByHeight();

        TileMap ruudut = TileMap.FromLevelAsset(kenttatiedostonNimi);
        ruudut.SetTileMethod('x', LuoSeina);
        ruudut.SetTileMethod('y', LuoPahkina);
        ruudut.SetTileMethod('z', LuoPelaaja);
        ruudut.SetTileMethod('o', LuoPelaaja2);
        ruudut.SetTileMethod('a', LuoAvain, "avain");
        ruudut.SetTileMethod('b', LuoBoksi);
        ruudut.SetTileMethod('v', LuoVipu, "vipuylos");
        ruudut.SetTileMethod('p', LuoPortaat);
        ruudut.SetTileMethod('k', LuoLiikkuva, 13);
        ruudut.SetTileMethod('#', LuoKeltainenVihu, 3);
        ruudut.SetTileMethod('3', LuoSininenVihu, 3);
        ruudut.SetTileMethod('m', LuoMaali);
        ruudut.SetTileMethod('t', LuoKeltainen, "keltainen");
        ruudut.SetTileMethod('s', LuoSininen, "sininen");
        ruudut.SetTileMethod('l', LuoPiikkilanka, "piikkilanka");
        ruudut.SetTileMethod('h', LuoHaamuseina, "lapinakyva");

        ruudut.Execute(RUUDUN_LEVEYS, RUUDUN_KORKEUS);


        Gravity = new Vector(0, PAINOVOIMA);

        PhoneBackButton.Listen(ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }


    /// <summary>
    /// Avaa seuraavan kentän, kun kentta numeroa kasvatetaan
    /// </summary>
    private void SeuraavaKentta()
    {
        ClearAll();
        if (kenttaNro == 1) LuoKentta("kentta");
        if (kenttaNro == 2) LuoKentta("kentta2");
        if (kenttaNro == 3) LuoKentta("kentta3");
        if (kenttaNro == 4) VoittoValikko();
    }


    /// <summary>
    /// luo valikon
    /// </summary>
    /// <param name="viesti">Teksti mitä tahdotaan</param>
    /// <param name="teksti">Viesti mikä tahdotaan</param>
    public void Valikko(string viesti, string teksti)
    {
        ClearAll();
        Level.Background.Image = LoadImage("tausta");
        Level.Background.ScaleToLevelByHeight();
        MessageDisplay.Y = Screen.Top - 100;
        MessageDisplay.X = Screen.Right - 100;
        MessageDisplay.Add(viesti);
        valikonKohdat = new List<Label>();
        Label kohta1 = new Label("Aloita peli");
        kohta1.Position = new Vector(0, 35);
        valikonKohdat.Add(kohta1);

        Label kohta2 = new Label("Lopeta peli");
        kohta2.Position = new Vector(0, 0);
        valikonKohdat.Add(kohta2);
        foreach (Label valikonKohta in valikonKohdat)
        {
            Add(valikonKohta);
        }

        Level.Background.Color = Color.DarkGray;
        Mouse.ListenOn(kohta1, MouseButton.Left, ButtonState.Pressed, AloitaPeli, null);
        Mouse.ListenOn(kohta2, MouseButton.Left, ButtonState.Pressed, Exit, null);
        Mouse.ListenMovement(1.0, ValikossaLiikkuminen, null);
    }


    /// <summary>
    /// Valikko joka näytetään jos voitetaan
    /// </summary>
    public void VoittoValikko()
    {
        Valikko("Voitit pelin!", "Aloita uusi peli");
    }


    /// <summary>
    /// Valikko joka näytetään jos hävitään
    /// </summary>
    public void HaviamisValikko()
    {
        Valikko("Hävisit pelin.", "Pelaa uudestaan");
    }


    /// <summary>
    /// Muuttaa tekstin väriä, jos hiiri on sen kohdalla
    /// </summary>
    void ValikossaLiikkuminen()
    {
        foreach (Label kohta in valikonKohdat)
        {
            if (Mouse.IsCursorOn(kohta))
            {
                kohta.TextColor = Color.BlueGray;
            }
            else
            {
                kohta.TextColor = Color.AshGray;
            }
        }
    }


    /// <summary>
    /// Tyhjentää kentän ja aloittaa seuraavan pelin
    /// </summary>
    public void AloitaPeli()
    {
        ClearAll();
        SeuraavaKentta();
        Gravity = new Vector(0, PAINOVOIMA);
    }


    /// <summary>
    /// Luo palikan, jota voi liikuttaa ja jonka päältä voi pompata
    /// </summary>
    /// <param name="paikka">Olion aloituspaikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    public void LuoPahkina(Vector paikka, double leveys, double korkeus)
    {
        pahkina = new Pahkina(leveys, korkeus, 1);
        pahkina.Position = paikka;
        pahkina.MakeStatic();
        pahkina.Image = pahkinanKuva;
        Add(pahkina);
    }


    /// <summary>
    /// luodaan ilmestyvä palikka, joka ilmestyy kun vipuun törmätään
    /// </summary>
    /// <param name="paikka">Olion aloituspaikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    public void LuoPortaat(Vector paikka, double leveys, double korkeus)
    {
        portaat = new PhysicsObject(leveys, korkeus);
        portaat.Position = paikka;
        portaat.MakeStatic();
        portaat.Image = portaatKuva; //uus kuva?
    }


    /// <summary>
    /// luodaan liikkuva pähkinä, mitä pelaajat voivat työntää
    /// </summary>
    /// <param name="paikka">Olion aloituspaikka kentällä</param>
    /// <param name="leveys">Olion leveys kentällä</param>
    /// <param name="korkeus">Olion korkeus kentällä</param>
    public void LuoBoksi(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject boksi = new PhysicsObject(leveys-1, korkeus-1);
        boksi.Position = paikka;
        boksi.Image = LoadImage("pahkina");
        boksi.Shape = Shape.Rectangle;
        boksi.Mass = 100;
        boksi.LinearDamping = 0;
        
        Add(boksi);
    }


    /// <summary>
    /// luodaan maali, josta päästään seuraavaan kenttään tai lopetetaan peli
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    public void LuoMaali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maali = new PhysicsObject(leveys, korkeus);
        maali.Position = paikka;
        maali.Tag = "maali";
        maali.Image = maalinKuva;
        maali.Color = Color.Black;
        maali.Shape = Shape.Rectangle;
        maali.MakeStatic();
        Add(maali);
    }


    /// <summary>
    /// luodaan normaali seinä
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    public void LuoSeina(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject suklaa = new PhysicsObject(leveys, korkeus);
        suklaa.Position = paikka;
        suklaa.Image = suklaanKuva;
        suklaa.MakeStatic();
        Add(suklaa);
    }


    /// <summary>
    /// Luodaan avain, joka tuhoaa oven.
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    /// <param name="kuvanNimi">Olion kuva</param>
    public void LuoAvain(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject kerattava = new PhysicsObject(leveys + 10, korkeus + 10);
        kerattava.Position = paikka;
        kerattava.Tag = "kerattava";
        kerattava.MakeStatic();
        kerattava.Image = LoadImage(kuvanNimi);
        Add(kerattava);
    }


    /// <summary>
    /// luodaan seinä, jonka takana molemmat pelaajat voivat kävellä
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    /// <param name="kuvanNimi">Olion kuva</param>
    public void LuoHaamuseina(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject haamuseina = new PhysicsObject(leveys, korkeus);
        haamuseina.Position = paikka;
        haamuseina.MakeStatic();
        haamuseina.Image = haamuseinaKuva;
        haamuseina.IgnoresCollisionResponse = true;
        Add(haamuseina);
            
    }


    /// <summary>
    /// Luodaan vipu johon pelaaja voi törmätä
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    /// <param name="kuvanNimi">Olion kuva</param>
    public void LuoVipu(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject vipu = new PhysicsObject(leveys, korkeus);
        vipu.Position = paikka;
        vipu.Tag = "vipu";
        vipu.MakeStatic();
        vipu.Image = LoadImage(kuvanNimi);
        Add(vipu);
    }


    /// <summary>
    /// luodaan "hissi" peliin joka liikuu
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    /// <param name="liikemaara">Olion liikkemäärä</param>
    public void LuoLiikkuva(Vector paikka, double leveys, double korkeus, int liikemaara)
    {
        PhysicsObject liikkuva = new PhysicsObject(leveys+40, korkeus);
        liikkuva.Shape = Shape.Rectangle;
        liikkuva.Position = paikka;
        liikkuva.Image = LoadImage("suklaati");
        liikkuva.CanRotate = false;
        Add(liikkuva);

        PathFollowerBrain pfb = new PathFollowerBrain();
        List<Vector> reitti = new List<Vector>();
        reitti.Add(liikkuva.Position);
        Vector seuraavaPiste = liikkuva.Position + new Vector(0, liikemaara * RUUDUN_KORKEUS);
        reitti.Add(seuraavaPiste);
        pfb.Path = reitti;
        pfb.Loop = true;
        pfb.Speed = 100;
        liikkuva.Brain = pfb;
    }


    /// <summary>
    /// luodaan keltainen vihollinen, johon keltainen pelaaja voi koskea kuolematta.
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    /// <param name="liikemaara">Olion liikkemäärä</param>
    public void LuoKeltainenVihu(Vector paikka, double leveys, double korkeus, int liikemaara)
    {
        PlatformCharacter keltainenVihu = new PlatformCharacter(leveys-4, korkeus-4);
        keltainenVihu.Shape = Shape.Circle;
        keltainenVihu.Position = paikka;
        //keltainenVihu.Image = LoadImage("keltainenVihu");

        keltainenVihu.AnimWalk = new Animation(LoadImages( "kvihu2", "kvihu1","kvihu3", "kvihu4"));
        keltainenVihu.AnimWalk.FPS = 15;

        keltainenVihu.Tag = "keltainenVihu";

        keltainenVihu.CanRotate = false;
        Add(keltainenVihu);

        PathFollowerBrain pfbk = new PathFollowerBrain();
        List<Vector> reittik = new List<Vector>();
        reittik.Add(keltainenVihu.Position);
        Vector seuraavaPiste = keltainenVihu.Position + new Vector(liikemaara * RUUDUN_LEVEYS, 0);
        reittik.Add(seuraavaPiste);
        pfbk.Path = reittik;
        pfbk.Loop = true;
        pfbk.Speed = 100;
        keltainenVihu.Brain = pfbk;
    }


    /// <summary>
    /// Luodaan sininen vihollinen, mihin sininen pelaaja voi koskea kuolematta
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    /// <param name="liikemaara">Olion liikkemäärä</param>
    public void LuoSininenVihu(Vector paikka, double leveys, double korkeus, int liikemaara)
    {
        PlatformCharacter sininenVihu = new PlatformCharacter(leveys-4, korkeus-4);
        sininenVihu.Shape = Shape.Circle;
        sininenVihu.Position = paikka;
        //sininenVihu.Image = LoadImage("sininenVihu");
        sininenVihu.Tag = "sininenVihu";

        sininenVihu.CanRotate = false;
        Add(sininenVihu);
        sininenVihu.AnimWalk = new Animation(LoadImages("svihu2", "svihu1", "svihu3", "svihu4"));
        sininenVihu.AnimWalk.FPS = 15;


        PathFollowerBrain pfbs = new PathFollowerBrain();
        List<Vector> reittis = new List<Vector>();
        reittis.Add(sininenVihu.Position);
        Vector seuraavaPiste = sininenVihu.Position + new Vector(liikemaara * RUUDUN_LEVEYS, 0);
        reittis.Add(seuraavaPiste);
        pfbs.Path = reittis;
        pfbs.Loop = true;
        pfbs.Speed = 100;
        sininenVihu.Brain = pfbs;
    }


    /// <summary>
    /// luodaan keltaiset salamat, mihin keltainen pelaaja voi koskea kuolematta
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    /// <param name="kuvanNimi">Olion kuva</param>
    public void LuoKeltainen(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject keltainen = new PhysicsObject(leveys, korkeus);
        keltainen.Position = paikka;
        keltainen.Tag = "keltainen";

        keltainen.MakeStatic();
        //punainen.Image = punaisenKuva;
        keltainen.Animation = new Animation(keltaisenAnimaatio);
        keltainen.Animation.Start();
        keltainen.Animation.FPS = 2;
        Add(keltainen);
    }


    /// <summary>
    /// luodaan vesi, mihin sininen pelaaja voi koskea kuolematta
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    /// <param name="kuvanNimi">Olion kuva</param>
    public void LuoSininen(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject sininen = new PhysicsObject(leveys, korkeus);
        sininen.Position = paikka;
        sininen.Tag = "sininen";
        sininen.MakeStatic();
        //sininen.Image = sinisenKuva;
        sininen.Animation = new Animation(sinisenAnimaatio);
        sininen.Animation.Start();
        sininen.Animation.FPS = 15;
        Add(sininen);
    }


    /// <summary>
    /// Luodaan piikkilanka mihin kumpikaan pelaaja ei voi koskea kuolematta.
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    /// <param name="kuvanNimi">Olion kuva</param>
    public void LuoPiikkilanka(Vector paikka, double leveys, double korkeus, string kuvanNimi)
    {
        PhysicsObject piikkilanka = new PhysicsObject(leveys, korkeus);
        piikkilanka.Position = paikka;
        piikkilanka.Tag = "piikkilanka";
        piikkilanka.MakeStatic();
        piikkilanka.Image = piikkilankaKuva;

        Add(piikkilanka);
    }


    /// <summary>
    /// luodaan sininen pelaaja
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    public void LuoPelaaja(Vector paikka, double leveys, double korkeus)
    {
        pelaaja = new PlatformCharacter(leveys - 4, korkeus - 4);
        pelaaja.Position = paikka;
        pelaaja.Shape = Shape.Circle;
        pelaaja.AnimWalk = new Animation(LoadImages("sp2", "sp1", "sp3", "sp4"));
        pelaaja.AnimIdle = new Animation(LoadImages("sp3"));
        pelaaja.AnimJump = new Animation(LoadImages("syhyppy"));
        pelaaja.AnimFall = new Animation(LoadImages("sahyppy"));
        pelaaja.AnimWalk.FPS =15;
        pelaaja.CollisionIgnoreGroup = 1;


        AddCollisionHandler(pelaaja, "kerattava", PelaajaTormasiKerattavaan);
        AddCollisionHandler(pelaaja, "maali", PelaajaTormasiMaaliin);
        AddCollisionHandler(pelaaja, "keltainen", PelaajaTormasiTappavaan);
        AddCollisionHandler(pelaaja, "piikkilanka", PelaajaTormasiTappavaan);
        AddCollisionHandler(pelaaja, "vipu", PelaajaTormasiVipuun);
        AddCollisionHandler(pelaaja, "keltainenVihu", PelaajaTormasiTappavaan);
        


        Keyboard.Listen(Key.Right, ButtonState.Down, Liikuta, "Liikuta pelaajaa oikealle", pelaaja, LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Left, ButtonState.Down, Liikuta, "Liikuta pelaajaa vasemmalle", pelaaja, -LIIKUTUSVOIMA);
        Keyboard.Listen(Key.Up, ButtonState.Pressed, Hyppaa, "Hyppää", pelaaja, HYPPYVOIMA) ;

        Add(pelaaja);
    }


    /// <summary>
    /// Luodaan keltainen pelaaja
    /// </summary>
    /// <param name="paikka">Olion paikka kentällä</param>
    /// <param name="leveys">Olion leveys</param>
    /// <param name="korkeus">Olion korkeus</param>
    public void LuoPelaaja2(Vector paikka, double leveys, double korkeus)
    {
        pelaaja2 = new PlatformCharacter(leveys - 4, korkeus - 4);
        pelaaja2.Position = paikka;
        pelaaja2.Shape = Shape.Circle;
        pelaaja2.AnimWalk = new Animation(LoadImages("kp2", "kp1", "kp3", "kp4"));
        pelaaja2.AnimIdle = new Animation(LoadImages("kp3"));
        pelaaja2.AnimJump = new Animation(LoadImages("kyhyppy"));
        pelaaja2.AnimFall = new Animation(LoadImages("kahyppy"));
        pelaaja2.AnimWalk.FPS = 15;
        pelaaja2.CollisionIgnoreGroup = 1;


        AddCollisionHandler(pelaaja2, "kerattava", PelaajaTormasiKerattavaan);
        AddCollisionHandler(pelaaja2, "maali", PelaajaTormasiMaaliin);
        AddCollisionHandler(pelaaja2, "sininen", PelaajaTormasiTappavaan);
        AddCollisionHandler(pelaaja2, "vipu", PelaajaTormasiVipuun);
        AddCollisionHandler(pelaaja2, "piikkilanka", PelaajaTormasiTappavaan);
        AddCollisionHandler(pelaaja2, "sininenVihu", PelaajaTormasiTappavaan);


        Keyboard.Listen(Key.D, ButtonState.Down, Liikuta, "Liikuta pelaajaa oikealle", pelaaja2, LIIKUTUSVOIMA);
        Keyboard.Listen(Key.A, ButtonState.Down, Liikuta, "Liikuta pelaajaa vasemmalle", pelaaja2, -LIIKUTUSVOIMA);
        Keyboard.Listen(Key.W, ButtonState.Pressed, Hyppaa, "Hyppää", pelaaja2, HYPPYVOIMA);


        Add(pelaaja2);
    }


    /// <summary>
    /// pelaaja hyppää
    /// </summary>
    /// <param name="hahmo">Olio joka hyppää</param>
    /// <param name="voima">Voima millä olio hyppää</param>
    public void Hyppaa(PlatformCharacter hahmo, double voima)
    {
        SoundEffect hyppyAani = LoadSoundEffect("hop");
        hahmo.Jump(voima);
        hyppyAani.CreateSound();
        hyppyAani.Play();
    }


    /// <summary>
    /// liikuteltavaa liikutetaan 
    /// </summary>
    /// <param name="liikuteltavaOlio">Liikuteltava olio</param>
    /// <param name="suunta">Suunta johon olio liikkuu</param>
    public void Liikuta(PlatformCharacter liikuteltavaOlio, double suunta)
    {
        
        liikuteltavaOlio.Walk(suunta);
    }


    /// <summary>
    /// Pelaaja tormää kerättävään ja ovi tuhoutuu.
    /// </summary>
    /// <param name="tormaaja">Olio joka törmää</param>
    /// <param name="kohde">Törmättävä kohde</param>
    public void PelaajaTormasiKerattavaan(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        MessageDisplay.Add("Ovi aukesi!");
        pahkina.Destroy();
        kohde.Destroy();
    }


    /// <summary>
    /// Pelaaja tormää kerättävään ja porras ilmestyy.
    /// </summary>
    /// <param name="tormaaja">Olio joka törmää</param>
    /// <param name="kohde">Törmättävä kohde</param>
    public void PelaajaTormasiVipuun(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        MessageDisplay.Add("Porras ilmestyi!");
        Add(portaat);
        kohde.Destroy();
    }


    /// <summary>
    /// Pelaaja tormää maaliin ja laitetaan seuraava kenttä
    /// </summary>
    /// <param name="tormaaja">Olio joka törmää</param>
    /// <param name="kohde">Törmättävä kohde</param>
    public void PelaajaTormasiMaaliin(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        kenttaNro++;
        SeuraavaKentta();
    }


    /// <summary>
    /// Pelaaja kuolee ja näytetään häviämisvalikko
    /// </summary>
    /// <param name="tormaaja">pelaaja</param>
    /// <param name="kohde">Tuhoava olio</param>
    public void PelaajaTormasiTappavaan(PhysicsObject tormaaja, PhysicsObject kohde)
    {
        HaviamisValikko();
    }

}