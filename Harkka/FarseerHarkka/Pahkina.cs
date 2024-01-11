using System;
using Jypeli;

public class Pahkina : PhysicsObject
{
    private int ElamiaJaljella;

    public Pahkina(double width, double height, int elamat) : base(width, height)
    {
        this.ElamiaJaljella = elamat;
    }
    public void OtaVastaanOsuma()
    {
        ElamiaJaljella--;
        if (ElamiaJaljella < 0) this.Destroy();
    }
}
   