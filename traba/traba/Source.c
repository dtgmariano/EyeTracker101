#include <windows.h>
#include <stdio.h>
#include "stdlib.h"

typedef struct
{
	char data[50];
	struct no *proximo;
}no;
FILE *arquivo;


no *lista;
no *atual;

no *alocarNo()
{
	no *novoNo;
	novoNo = (no *)malloc(sizeof(no));
	novoNo->proximo = NULL;
	return novoNo;
}

char * colocaDataEmString()
{
	char nome[50];
	const time_t timer = time(NULL);
	int n;
	n = sprintf(nome, "%s", ctime(&timer));
	return nome;
}
void inserir(char nome[50]) // insere um novo nó na fila
{
	arquivo = fopen("F:\\trabalhofinal.txt", "a +");
	if (lista == NULL) // fila está vazia
	{
		lista = alocarNo();
		strcpy(lista->data, nome);
		fprintf(arquivo, "%s", nome);
	}
	else
	{
		atual = lista;
		if (atual->proximo == NULL) // A fila só tem um elemento
		{
			atual->proximo = alocarNo();
			atual = atual->proximo;
			strcpy(atual->data, nome);
			fprintf(arquivo, "%s \n", nome);
		}
		else // a fila tem mais de um elemento
		{
			while (atual->proximo != NULL) // vai pro fim da fila e insere o nó
			{
				atual = atual->proximo;
			}
			atual->proximo = alocarNo();
			atual = atual->proximo;
			strcpy(atual->data, nome);
			fprintf(arquivo, "%s \n", nome);
		}
	}fclose(arquivo);
}

void imprimir()
{
	system("cls");
	atual = lista;
	if (lista == NULL) // fila está vazia
	{
		printf("Lista vazia");
	}
	else
	{
		atual = lista;
		if (atual->proximo == NULL) // A fila só tem um elemento
		{
			printf("Data: %s \n", atual->data);
		}
		else // a fila tem mais de um elemento
		{
			do
			{
				printf("Data: %s \n", atual->data);
				atual = atual->proximo;
			} while (atual != NULL);
		}
	}


}

// Ler caractere
char SerialGetc(HANDLE *hCom)
{
	char rxchar;
	BOOL bReadRC;
	static DWORD iBytesRead;

	bReadRC = ReadFile(*hCom, &rxchar, 1, &iBytesRead, NULL);
	return rxchar;
}


// Escrever caractere
void SerialPutc(HANDLE hCom, char txchar)
{
	BOOL bWriteRC;
	static DWORD iBytesWritten;

	bWriteRC = WriteFile(hCom, &txchar, 1, &iBytesWritten, NULL);

	return;
}


// Ler string
char* SerialGets(HANDLE *hCom)
{
	static char rxstring[256];
	char c;
	int pos = 0;

	while (pos <= 255)
	{
		c = SerialGetc(hCom);
		if (c == 13) break;
		if (c == '\r') continue; // discard carriage return 
		rxstring[pos++] = c;
		if (c == '\n') break;

	}
	rxstring[pos] = 0;
	return rxstring;
}


// Escrever string
void SerialPuts(HANDLE *hCom, char *txstring)
{
	BOOL bWriteRC;
	static DWORD iBytesWritten;
	bWriteRC = WriteFile(*hCom, txstring, strlen(txstring), &iBytesWritten, NULL);
}

int main(int argc, char *argv[])
{

	char String[100];
	char DATA[50];
	int num, op1;
	DCB dcb;
	HANDLE hCom;
	BOOL fSuccess;
	LPCWSTR LpcCommPort = L"COM3";

	hCom = CreateFile(LpcCommPort,
		GENERIC_READ | GENERIC_WRITE,
		0,    // must be opened with exclusive-access
		NULL, // no security attributes
		OPEN_EXISTING, // must use OPEN_EXISTING
		0,    // not overlapped I/O
		NULL  // hTemplate must be NULL for comm devices
		);

	if (hCom == INVALID_HANDLE_VALUE)
	{
		// Handle the error.
		printf("CreateFile failed with error %d.\n", GetLastError());
		system("PAUSE");
		return (1);
	}

	// Build on the current configuration, and skip setting the size
	// of the input and output buffers with SetupComm.

	fSuccess = GetCommState(hCom, &dcb);

	if (!fSuccess)
	{
		// Handle the error.
		printf("GetCommState failed with error %d.\n", GetLastError());
		system("PAUSE");
		return (2);
	}

	// Fill in DCB: 57,600 bps, 8 data bits, no parity, and 1 stop bit.

	dcb.BaudRate = CBR_9600;     // set the baud rate
	dcb.ByteSize = 8;             // data size, xmit, and rcv
	dcb.Parity = NOPARITY;        // no parity bit
	dcb.StopBits = ONESTOPBIT;    // one stop bit

	fSuccess = SetCommState(hCom, &dcb);

	//

	if (!fSuccess)
	{
		// Handle the error.
		printf("SetCommState failed with error %d.\n", GetLastError());
		system("PAUSE");
		return (3);
	}
	for (;;)
	{
		//Sleep(1550);
		//Sleep(1500);
		/* do
		{*/
		/*   system("cls");
		printf("#####################################################################\n");
		printf("#      Software para verificacao de interacao do botao no Semaforo. #\n");
		printf("#####################################################################\n");
		printf("Escolha uma opcao.\n");
		printf("Digite 1 para ver registros.\n");
		printf("Digite 2 para limpar registros.\n");
		printf("Digite 3 para ver funcionamento simultaneo do semafaro.\n");
		printf("Digite 4 para sair do programa.\n");*/
		Sleep(10000);
		strcpy(String, SerialGets(&hCom));
		num = atoi(String);
		/* scanf("%d",&op1);*/
		/*if(op1==1)
		{
		imprimir();
		}
		if(op1==2)
		{
		remove(arquivo);
		}
		if(op1==3)
		{*/


		if (num == 1)
		{
			strcpy(DATA, colocaDataEmString());
			inserir(DATA);
			imprimir();
		}
		/*}*/
		/*}


		}while(op1!=4);*/
	}

	CloseHandle(hCom);
	return (0);
}