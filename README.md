# EmguCV biblioteka

**EmguCV** je kros-platformska *wrapper* biblioteka oko **OpenCV-a**. Dozvoljava pozivanje OpenCV funkcija iz .NET kompatibilnih jezika.

Implementirane funkcionalnosti:
1) Brightness filter
2) Contrast filter
3) Filter za detekciju kontura na slici
4) Detekcija svih pravougaonike određene boje. Omogućiti korisniku da učita proizvoljnu sliku, da izabere boju i minimalnu veličini pravougaonika.

## Korisnički interfejs

Većina akcija se nalazi u toolbar meniju. Izborom `File -> Open` se učitava slika koja se prikazuje. Filteri su dostupni u `Filtering` meniju. Recimo, `Filtering -> Brightness filter / Increase` primenjuje filter povećanja osvetljenja, slično `Filtering -> Contrast filter` primenjuje filter promene kontrasta. Filteri mogu da se kombinuju. `Edit -> Triangle detection` je komanda za detekciju pravougaonika i iscrtavanje pronadjenih koji odgovaraju podešavanjima. U statusbar-u se biraju **boja i površina (u px)** ovih pravouganika.

![alt text][screenshot-none]

[screenshot-none]: meta/screenshot-none.png

## Brightness & Contrast filteri

`Filtering -> Brightness filter / Increase` se koristi za primenu filtera povećavanja osvetljena, o slično tome `Filtering -> Brightness filter / Decrease` za smanjenje. `Filtering -> Contrast filter` menja kontrast slike.

Za promenu osveteljenja poziva se `_GammaCorrect(value);` API, a za promenu kontrasta `_EqualizeHist();` kojim se normalizuje osvetljenje i povećava kontrast.

![alt text][screenshot-brightness]

[screenshot-brightness]: meta/screenshot-brightness.png

Na slici se može videti promena osvetljenja slike nakon primene filtera.

![alt text][screenshot-contrast]

[screenshot-contrast]: meta/screenshot-contrast.png

Slično, ovo je slika sa promenjenim kontrastom. Filteri su u ovom slučaju kombinovani jer je promena kontrasta primenjena nakon promene osvetljenja.

## Detekcija kontura

`Filtering -> Conture detection` filter se koristi za detekciju kontura na slici. Rezultat je grayscale slika sa iscrtanim konturama u beloj boji.

```c#
VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint();
Mat hier = new Mat();
CvInvoke.FindContours(bwImage, contours, hier, RetrType.External, ChainApproxMethod.ChainApproxSimple);
CvInvoke.DrawContours(returnImage, contours, -1, new MCvScalar(255, 0, 0));

return Bitmap2BitmapImage(returnImage.Bitmap);
```

Za detekciju kontura je neophodno klonirati sliku u **grayscale obliku**. Zatim, koristeći metodu pronalaženja kontura `ChainApproxMethod.ChainApproxSimple` detektovati prisutne konture na slici (i iscrtati ih naknadno). Za detekciju se koristi poziv `CvInvoke.FindContours`.

![alt text][screenshot-conture]

[screenshot-conture]: meta/screenshot-conture.png

## Detekcija pravouganika

`Edit -> Triangle detection` se koristi za detekciju pravouganika. Pre primene je neophodno navesti boju i minimalnu površinu pravouganika u px. Boja se bira color-picker elementom u statusbar-u (izabrana je crna), a površina se navodi pored (navedeno je 8000px).

```c#
#region find rectangles by contoure detection
using (VectorOfVectorOfPoint contours = new VectorOfVectorOfPoint())
{
    CvInvoke.FindContours(bwImage, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
    int count = contours.Size;
    for (int i = 0; i < count; i++)
    {
        using (VectorOfPoint contour = contours[i])
        using (VectorOfPoint approxContour = new VectorOfPoint())
        {
            CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * 0.05, true);
            double contourArea = CvInvoke.ContourArea(approxContour, false);
            if (contourArea > area) // only consider contours with area greater than "area"
            {
                if (approxContour.Size == 4) // contour has 4 vertices
                {
                    #region determine if all the angles in the contour are within [80, 100] degree
                    bool isRectangle = true;
                    Point[] pts = approxContour.ToArray();
                    LineSegment2D[] edges = PointCollection.PolyLine(pts, true);
                    for (int j = 0; j < edges.Length; j++)
                    {
                        double angle = Math.Abs(
                           edges[(j + 1) % edges.Length].GetExteriorAngleDegree(edges[j]));
                        if (angle < 80 || angle > 100)
                        {
                            isRectangle = false;
                            break;
                        }
                    }
                    #endregion

                    if (isRectangle) rectangleList.Add(CvInvoke.MinAreaRect(approxContour));
                }
            }
        }
    }
}
#endregion

#region draw rectangles on-top of source image
Image<Bgr, Byte> triangleRectangleImage = tempImage.CopyBlank();
foreach (RotatedRect box in rectangleList)
    triangleRectangleImage.Draw(box, new Bgr(Color.DarkMagenta), 2);
#endregion
```

Slično kao u prošlom primeru, prvo se pronalaze sve konture na slici. Osnova je ponovno **grayscale slika**, s tim što je u ovom slučaju na početku urađeno i **filtriranje boja** kako bi se razmatrali samo pravouganici odgovarajuće boje. Zatim, treba uzeti u obzir samo one konture koje su **veće površine od zadate** - površina se računa za svaku konturu u ovom koraku. Nakon toga, treba razmotriti samo **konture koje imaju četiri temena**. Na kraju, treba proveriti da li su **uglovi između temena u opsegu [80, 100] stepeni**.

Lista kontura koje odgovaraju uslovima se iscrtava na crnoj pozadini.

![alt text][screenshot-triangles]

[screenshot-triangles]: meta/screenshot-triangles.png

Podešavanja su boja-crna, minimalna površina-8000px. Na slici u primeru, svi pravouganici osim jednog su crne boje. Međutim, najmanji pravouganik crne boje ne zadovoljava uslov minimalne površine jer je njegova površina oko 5000px. Na kraju, preko crne pozadine su iscrtani svi prepoznati pravouganici.

![alt text][screenshot-triangles-result]

[screenshot-triangles-result]: meta/screenshot-triangles-result.png
