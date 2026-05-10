    - vettore costante { 1 , 2 , 3 } come operando di espressione (non esiste in C) la sintassi è simile alle init:
         eg v = {1,2,3}
    - il vettore costante può inizializzare dinamicamente le struct eg s = { .a = 2 }
         tuttavia non è possibile inizializzare tramite assegnamento pe in questo caso s.b = 3 genera eccezione (s.a = 3 è valido)
    - operazioni tra vettori(che sia un array nativo C o un variant che punta a un vettore è indifferente):
         - un'operazione che ritorna un variant è sempre per valore a meno che non si prependa l'operatore *
           esempio v1 = v2 (per riferimento) v1 = *v2 (per valore)
         - v2=(v1+=3) ( vettore + scalare ) incrementa v[i] di 3 e v2 contiene un ref a v1
         - l'operazione v3=v1*v2 è il prodotto righe per colonne
         - l'operazione v3*=v2 è il prodotto elemento per elemento (le dimensioni DEVONO matchare)
         - l'operatore << fonde gli array per RIGA eg v1 = { 1, 2 } v2 = {3 , 4 }    v3 = (v1 << v2 ) {1,2,3,4}
         - l'operatore >> fonde gli array per RIGA eg v1 = { 1, 2 } v2 = {3 , 4 }    v3 = (v1 >> v2 ) { {1,3} , {2,4} }
         - gli operatori unari agiscono su ogni singolo elemento (cast incluso)
         - l'operatore ~ traspone una matrice tranne nel caso l'array sia di interi in questo caso effettua la complementazione
             per trasporre una matrice di interi occorre castare a double mi = { {1,2}, {3,4}} mit = (int)~((double)mi)
            