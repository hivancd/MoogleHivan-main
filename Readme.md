# Moogle!

![](moogle.png)

> Hivan Cañizares Díaz
> Grupo C111
> Proyecto de Programación I.
> Facultad de Matemática y Computación - Universidad de La Habana.
> Cursos 2021, 2022.

Moogle! es una aplicación cuyo propósito es buscar inteligentemente una expresión en un conjunto de documentos.

## ¿Comó utilizar Moogle!?

Introduzca la expresión que desea buscar en nuestra pintoresca interfaz y Moogle! encontrará todos los textos que coincidan total o parcialmente con su búsqueda(El directorio en que se encuentran los documentos es E:\Prog\moogle\moogle-main\Content). Moogle! le enseñará los nombres de los documentos en los que aparecen las palabras de su búsqueda junto con un pedazo de 300 carácteres de el texto alrededor de la primera aparición de la expresión en este.

### Sugerencias

Moogle! cuenta con un sofisticado sistema de sugerencias el cual en caso de que su búsqueda tenga ninguna o pocas coincidencias o que se encuentre una palabra que le pueda interesar en su búsqueda aparecerá señaladas palabras que tienen parecido con la expresión que introdujo. 

## ¿Comó funciona Moogle!?

Moogle! posee un avanzadisimo sistema de búsqueda por matrices númericas cuyos secretos se pueden desvelar inspeccionando el código escrito en el archivo moogle-main\MoogleEngine\MatricesWork.cs una vez en este archivo se puede observar que forma parte de el namespace MoogleEngine y que contiene la clase homónima con la carpeta. En esta clase se pueden encontrar los métodos que permiten una búsqueda supereficiente basada en matrices.

## ¿Comó se construye la matriz de búsqueda?

### Dictionary<string, int>[] GetWordDictionarys(string[] files)

Este método es el primer paso para la construcción de la matriz númerica, recibe un array con los filenames de la carpeta Content y después de ir por cada uno de estos archivos devuelve un array de diccionarios donde cada diccionario corresponde con un texto de el mismo index en el array de archivos, cada string que sirve como Keys para los diccionarios es una palabra que aparece en ese texto y el entero correspondiente es la cantidad de veces que se encuentra en el texto.

### List<string> GetAllTheWords(Dictionary<string, int>[] Dictionarys)

Este método recibe el diccionario creado anteriormente y a partir de el crea una lista con todas las palabras de todos los archivos ciclando por las Keys de cada diccionario.

### int[,] GetMatrix(string[] files, Dictionary<string, int>[] Dictionarys, List<string> AllTheWords)

Este método crea una matriz bidimensional de archivos contra palabras. Las posiciones de esta matriz indican cuantas veces aparece una palabra en un texto.

### double[,] tf_idf(int[,] Matrix)

Este método construye una matriz de tf-idf a partir de la matriz creada anteriormente:

#### Tf-Idf

Es un valor estadístico que refleja la importancia de una palabra en un conjunto de documentos se calcula con la fórmula:

n/N·log(D/d)

n(cant. de veces que la palabra aparece en el texto)
N(Total de palabras en el texto)
D(cant. de docs.)
d(cant. de docs en los que aparece la palabra)

(Fuente: Wikipedia en inglés)

#### int[] GetTotalWords(int[,] Matrix)

Para calcular el tf-idf de las palabras es necesario saber la cantidad de palabras en total que tiene cada método para eso es este método. El cual devuelve un array de enteros con estos datos que se extraen a partir de la matriz.

#### int[] GetNumberOfDocsEachWordAppears(int[,] Matrix)

Este método devuelve un array con la cantidad de documentos en los que aparece cada palabra estos datos serán necesarios para el cálculo de el tf-idf.

Una vez construida la matriz de búsqueda pasamos al siguiente paso.

## Operadores especiales

Moogle! cuenta con un sistema de operadores especiales que permiten especializar la búsqueda:

### Palabras no permitidas y necesarias en la búsqueda 
### Método:  double[,] NotAllowedandNecessaryOp(string query, List<string> AllTheWords, double[,] Matrix)

El utilizar los símbolos ! y ^ en la búsqueda permite al usuario volver a la palabra que los sucede palabras prohibidas(no pueden aparecer en los documentos que se den como resultado) y necesarias(deben aparecer en los textos que se den como resultado) respectivamente. Para esto el método recorre la matriz de búsqueda y si el lugar correspondiente a la palabra prohibida es diferente de cero convierte a los valores de ese documento en cero. Si el lugar de la palabra necesaria es cero convierte todos los valores de ese documento en cero igualmente.

### Importancia
### Método: static double[,] GetImportance(string query, List<string> AllTheWords, double[,] Matrix)

El símbolo * aumenta la importancia de la palabra que precede según la cantidad de * que tenga, o sea que esa palabra tenga más influencia en el valor de búsqueda. Este método aumenta los valores de las palabras que tienen importancia según la cantidad de símbolos * multiplicando por 1+cant. de asteriscos a los valores que corresponden con esta palabra.

### Cercania
### static double[,] Cercania(double[,] Matrix, List<string> AllTheWords, string[] files, string query, Dictionary<string, int>[] Dictionarys)

Los símbolos ~ entre dos palabras indican que estas dos palabras deben aparecer más cerca por lo que mientras más cerca mayor es el valor de búsqueda. Este método procesa los símbolos ~. Primero identifica las dos palabras que deben estar cercanas y luego el método auxiliar GetTextIndexes toma las indexes de los textos en la matriz (los textos en los que aparecen ambas palabras) entrando en un ciclo por estos indexes. Con el método GetShortestDistance obtengo el valor de cercania de ese texto.

#### static double GetShortestDistance(string text, string LeftWord, string RightWord, Dictionary<string, int>[] Dictionarys, int index)

Primero creo dos listas que tienen los indexes de cada palabra en el texto que estoy analizando con el método GetIndexes, después procedo a restar todos los indexes  de uno con los de el otro quedandome  con los valores absolutos en otra lista, al menor valor de esta lista lo tomo y le sumo uno a su inverso y este valor es el que devuelvo para ese texto y que será multiplicado por cada valor de ese documento.

## La Query

Después de tener lista la matriz de los valores de búsqueda es hora de trabajar sobre la query.

### Método ProcessQuery 

El método ProcessQuery de la clase QueryProcessing toma el query y devuelve search_query quitándole los espacios en blanco irrelevantes, los símbolos de operadores y las stopwords con métodos auxiliares.

### Query a vectores y búsqueda vectorial

Es necesario llevar la query procesada a un vector de búsqueda para que pueda ser fácilmente calculado el valor final de cada búsqueda en cada archivo y esto se hace con el método Sentence2Vec el cuál recibe la query y retorna un array con unos y ceros donde cada posición con un 1 es la posición de la palabra en la lista Matriz. Teniendo la representación vectorial de la query y la matriz con los valores de búsqueda es necesario el método VecXMatrix que devuelve los valores de búsqueda para cada archivo de la oración en cuestión. Para calcular esto este método cicla por la primera dimensión de la matriz y luego por la segunda sumando los valores de la multiplicación de cada elemento de la fila por el elemnto de el vector (que siempre es uno), así obteniendo un array con el valor de la query en cada archivo.

## Resultados de búsqueda 

Con los datos que hemos obtenido ya podemos dar nuestros resultados de búsqueda.

### SearchItem[] GetSearchItems(double[] SearchValues)

Con el array de los valores de búsqueda este método crea una lista con todos los resultados diferentes de 0 de los SearchItems. Para crear los SearchItems necesita:

#### Score

Extraido de el array SearchValues.

#### Snippet

Este es sacado utilizando el método GetSnip que extrae una muestra de texto de 300 caracteres de la primera ocurrencia de una de las palabras de la query en el texto

#### Nombre del archivo 

Este es sacado de la lista filenames con la dirección de los archivos

### string GetSuggestion(List<string> AllTheWords, string query)

El método GetSuggestion de la clase suggestion obtiene la sugerencia de búsqueda. Para esto recorre la lista de todas las palabras de los documentos y mediante el método IsSimilar determina cuan cercana es cada una con las palabras de la query quedandose con las más similares y devolviendolas como la sugerencia.

Una vez con los SearchItems y con las suggerencias se puede devolver el SearchResult y podemos dar por concluida la búsqueda.