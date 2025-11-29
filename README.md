# C-Project
FIndmy C# Project in here !
Using System ;
Console.Write("Masukkan umur: ");
if (!int.TryParse(Console.ReadLine(), out int umur))
{
    Console.WriteLine("Input tidak valid");
    return;
}

Console.Write("Masukkan status (1=member, 0=non-member): ");
if (!int.TryParse(Console.ReadLine(), out int status))
{
    Console.WriteLine("Input tidak valid");
    return;
}

if (status != 0 && status != 1)
{
    Console.WriteLine("Input tidak valid");
    return;
}

int harga;
if (umur < 12)
{
    harga = (status == 1) ? 20 : 25;
}
else
{
    harga = (status == 1) ? 40 : 50;
}

Console.WriteLine(harga);

