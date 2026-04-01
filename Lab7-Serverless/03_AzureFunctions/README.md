# LAB 7b - Azure Functions

*Zadání: Vytvoře Azure Function, která podle datumu vrátí znamení zvěrokruhu. Nasaďte ji do MS Azure, otestujte a prozkoumejte Azure Portál*

1. Vytvořte novou Azure Function

![](img/functions01.png)
![](img/functions02.png)
![](img/functions03.png)

2. Ujistěte se, že ve Visual Studiu máte Azure Development workload 

![](img/functions00.png)

3. Založte projekt ve Visual Studiu se šablonou Azure Function

![](img/functions05.png)
![](img/functions06.png)

4. Můžete použít [přiložený kód](ZodiacFunction/ZodiacFunction.cs)

5. Lokálně funkci otestujte v prohlížeči. Prozkoumejte, jak se volání chová při neplatném datumu URL. 
[Optional] Zkuste odeslat POST požadavek místo GET. Datum použijte v těle

![](img/functions07.png)
![](img/functions07b.png)

6. Vypublikujte projekt do MS Azure

![](img/functions08.png)
![](img/functions09.png)
![](img/functions09b.png)

7. Prozkoumejte Azure portál, najděte URL Azure Funkce a zkuste ji zavolat "v produkci"

![](img/functions10.png)

