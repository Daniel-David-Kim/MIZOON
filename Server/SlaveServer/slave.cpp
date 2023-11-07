/* ���� ���̺귯�� */
#include <WinSock2.h>
#include <WS2tcpip.h>
#include <locale.h>
#include <stdio.h>
#include <iostream>

/* ����� ���� ���̺귯�� */
#include "../com/psi.h"
#include "../com/packet.h"
#include "../com/common.h"
#include "strobj.h"

/* ���̺귯�� ��ũ */
#pragma comment(lib, "ws2_32")

#define SERVERSTORAGE "E:\\Mark12\\testArchive\\sock\\"
#define BUFSIZE     2048
#define MAX_SOCKET  32

typedef struct tag_server_info 
{
    HANDLE      hPipe;
    LPCTSTR     pipeName;
    TCHAR       serverName[64];
    int         port;
    int         index;
} SERVER_INFO;

typedef struct tag_socket_info
{
    SOCKET              sock;
    struct sockaddr_in  addr;
    int                 addrlen;
    TCHAR               serverName[64];
} SOCKET_INFO;

SERVER_INFO* serverInfo = NULL;
SOCKET_INFO* g_SocketInfoArray[MAX_SOCKET];
int si_cursor;

bool AddSocketInfo(SOCKET sock, struct sockaddr_in& addr, int addrlen);
bool RemoveSocketInfo(SOCKET* sock);
BOOL SendChat(const char*, SOCKET*);
BOOL SendTextFile(const char*, SOCKET*);
BOOL SendImageFile(const char*, SOCKET*);
SERVER_INFO* CheckServerType(char* type);
HANDLE TryToConnectPipe(LPCTSTR lpszPipeName, int timeout);
BOOL ChangePipeMode(HANDLE hPipe, DWORD dwMode);
BOOL WritePipeMessage(HANDLE hPipe, void* buf, DWORD writeBytes, DWORD cbWritten);
BOOL ReadPipeMessage(HANDLE hPipe, void* buf, DWORD readBytes, DWORD cbRead);
DWORD SlaveMain(LPVOID arg);
void err_display(const char* title);
void err_quit(const char* title);

int main(int argc, char* argv[])
{
    setlocale(LC_ALL, "korean");

    si_cursor = 0;
    HANDLE hPipe;
    BOOL result = FALSE;
    DWORD writeBytes, readBytes;
    int timeout = 20000;

    // ---------------------------------
    /* ������ ���� �ʱ�ȭ �۾� */
    // ---------------------------------

    // � ������ ����ϴ��� üũ�ϴ� �κ�
    serverInfo = CheckServerType(argv[1]);
    if (NULL == serverInfo) err_quit("CheckServerType()");

    // ������ ���� �õ�
    hPipe = TryToConnectPipe(serverInfo->pipeName, timeout);
    if (NULL == hPipe) err_quit("TryToConnectPipe()");
    serverInfo->hPipe = hPipe;

    // ������ ���ῡ ���������Ƿ� �޽��� �б� ���� ��ȯ
    result = ChangePipeMode(hPipe, (PIPE_READMODE_MESSAGE));
    if (FALSE == result) err_quit("ChangePipeMode()");
    
    // ���� �ʱ� ���� �޽����� �ۼ��Ѵ�.
    PIPE_MSG pm;
    pm.type = PIPE_TYPE_INIT;
    sprintf(pm.userName, "%d", serverInfo->port);
    strcpy(pm.msg, "Ȱ��ȭ ����");
    
    writeBytes = PIPE_MSG_SIZE;
    result = WritePipeMessage(hPipe, &pm, writeBytes, 0);
    if (FALSE == result) err_quit("WritePipeMessage()");

    // ���� �ʱ� ���� �޽����� �д´�.
    readBytes = (int)strlen(pm.msg);
    result = ReadPipeMessage(hPipe, &pm, readBytes, 0);
    if (FALSE == result) err_quit("ReadPipeMessage");

    // ---------------------------------
    /* ����-Ŭ���̾�Ʈ ��ƾ ���� */
    // ---------------------------------

    wprintf(TEXT("%s ���� ����!\n"), serverInfo->serverName);
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2, 2), &wsa) != 0) err_quit("WSAStartup()");

    SOCKET listen_sock = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (listen_sock == INVALID_SOCKET) err_quit("listener socket()");

    int retval;
    struct sockaddr_in serveraddr;
    serveraddr.sin_family = AF_INET;
    serveraddr.sin_port = htons(serverInfo->port);
    serveraddr.sin_addr.s_addr = INADDR_ANY;

    retval = bind(listen_sock, (struct sockaddr*)&serveraddr, sizeof(serveraddr));
    if (retval == SOCKET_ERROR) err_quit("bind()");

    retval = listen(listen_sock, SOMAXCONN);
    if (retval == SOCKET_ERROR) err_quit("listen()");

    struct sockaddr_in clientaddr;
    HANDLE hThread;
    SOCKET client_sock;
    int addrlen;
    
    /* ���� �۾� ������ ó�� */
    while (true)
    {
        addrlen = sizeof(clientaddr);
        client_sock = accept(listen_sock, (struct sockaddr*)&clientaddr, &addrlen);
        if (INVALID_SOCKET == client_sock)
        {
            err_display("accept()");
            break;
        }

        if (si_cursor >= MAX_SOCKET)
        {
            printf("Ŭ���̾�Ʈ �ִ� ���� ���� �Ѱ踦 �ʰ��߽��ϴ�.\n");
            continue;
        }

        char addr[INET_ADDRSTRLEN];
        inet_ntop(AF_INET, &clientaddr.sin_addr, addr, sizeof(addr));
        printf("[SLAVE SERVER] Ŭ���̾�Ʈ ���� [%s:%d]\n", addr, ntohs(clientaddr.sin_port));

        if (!AddSocketInfo(client_sock, clientaddr, sizeof(clientaddr)))
            closesocket(client_sock);

        hThread = CreateThread(NULL, 0, SlaveMain, (LPVOID)client_sock, 0, NULL);
        if (NULL == hThread)
        {
            printf("Ŭ���̾�Ʈ ������ ���� ����! ���� ���� ����!\n");
            closesocket(client_sock);
        }
        else CloseHandle(hThread);
    }

    /* ������ ���� ���� */
    FlushFileBuffers(hPipe);
    DisconnectNamedPipe(hPipe);
    CloseHandle(hPipe);

    /* ���� ���� ���� */
    closesocket(listen_sock);
    WSACleanup();
    return 0;
}

bool AddSocketInfo(SOCKET sock, struct sockaddr_in& addr, int addrlen)
{
    if (si_cursor >= MAX_SOCKET)
    {
        printf("[����] ���� ������ ���� �� �߰��� �� �����ϴ�.");
        return false;
    }

    SOCKET_INFO* ptr = new SOCKET_INFO;
    if (NULL == ptr)
    {
        printf("[����] �޸𸮰� �����մϴ�.");
        return false;
    }

    ptr->sock = sock;
    ptr->addr = addr;
    ptr->addrlen = addrlen;
    g_SocketInfoArray[si_cursor++] = ptr;
    return true;
}

bool RemoveSocketInfo(SOCKET* socket)
{
    for (int i = 0; i < si_cursor; ++i)
    {
        if (g_SocketInfoArray[i]->sock == *socket)
        {
            closesocket(g_SocketInfoArray[i]->sock);
            delete g_SocketInfoArray[i];

            if (i != (si_cursor - 1))
                g_SocketInfoArray[i] = g_SocketInfoArray[si_cursor - 1];

            --si_cursor;
            return true;
        }
    }

    return false;
}

SERVER_INFO* CheckServerType(char* type)
{
    const char* types[PIPE_SERVER_COUNT] = {
        "asgard", "midgard"
    };

    for (int i = 0; i < PIPE_SERVER_COUNT; ++i)
    {
        if (strcmp(type, types[i]) == 0)
        {
            SERVER_INFO* si = new SERVER_INFO;
            si->pipeName = lpszPipeNames[i];
            lstrcpy(si->serverName, lpTitles[i]);
            si->port = ((int)PIPE_SERVER_PORT::ASGARD) + i;
            si->index = i;
            wprintf(TEXT("���� �̸� : %s, ������ �̸� : %s, ��Ʈ ��ȣ = %d\n"), si->serverName, si->pipeName, si->port);
            return si;
        }
    }

    return NULL;
}

HANDLE TryToConnectPipe(LPCTSTR lpszPipeName, int timeout)
{
    HANDLE hPipe = NULL;
    while (true)
    {
        // ������ ���� �õ�
        hPipe = CreateFile(
            lpszPipeName,   // ������ �̸�
            GENERIC_READ |
            GENERIC_WRITE,  // �б� & ���� ���� �ޱ�
            0,              // ���� ��� ���� (�� ��� �������� ����)
            NULL,           // �⺻ ��ť��Ƽ �Ӽ���
            OPEN_EXISTING,  // �������� �̹� ������ ��쿡�� ������ ����
            0,              // �������� ��Ÿ �Ӽ� �ο�
            NULL);          // ���ø� ���� ����

        // ���� �������� �޾ƿ��µ� �������� ��� ���� ������ Ż���Ѵ�.
        if (INVALID_HANDLE_VALUE != hPipe)
            break;

        // ���� �ڵ尡 ERROR_PIPE_BUSY �� ��� �����Ѵ�.
        if (GetLastError() != ERROR_PIPE_BUSY)
        {
            wprintf(TEXT("������ ���� ����. (���� �ڵ� = %d)\n"), GetLastError());
            exit(1);
        }

        // �������� �����ϱ� ���� ������ �ð� ���� ����ϴٰ� ������� ���� �� ���� ����
        if (!WaitNamedPipe(lpszPipeName, timeout))
        {
            MessageBox(NULL, TEXT(""), TEXT(""), MB_ICONERROR);
            exit(1);
        }
    }

    return hPipe;
}

BOOL ChangePipeMode(HANDLE hPipe, DWORD dwMode)
{
    BOOL result;

    result = SetNamedPipeHandleState(
        hPipe,    // ������ �ڵ�
        &dwMode,  // �������� ���� ���ο� ��� 
        NULL,     
        NULL);    
    if (!result)
    {
        wprintf(TEXT("SetNamedPipeHandleState failed. GLE=%d\n"), GetLastError());
        return FALSE;
    }

    return TRUE;
}

BOOL WritePipeMessage(HANDLE hPipe, void* buf, DWORD writeBytes, DWORD cbWritten)
{
    BOOL result;

    result = WriteFile(
            hPipe,           // ������ �ڵ�
            buf,             // �޽��� ����
            writeBytes,      // �޽��� ����
            &cbWritten,      // �����µ� �������� ��� ����Ʈ ���� ��ϵ� 
            NULL);           // ��ø I/O ����
    if (!result)
    {
        wprintf(TEXT("WriteFile to pipe failed. GLE=%d\n"), GetLastError());
        return FALSE;
    }

    // ����� ������ ������ �޽����� ����
    wprintf(TEXT("[SLAVE SERVER:PIPE] %d����Ʈ ���� �Ϸ�\n"), writeBytes);
    return TRUE;
}

BOOL ReadPipeMessage(HANDLE hPipe, void* buf, DWORD readBytes, DWORD cbRead)
{
    BOOL result;

    // ������ �б�
    result = ReadFile(
            hPipe,      // ������ �ڵ�
            buf,        // ������ ���� ����
            readBytes,  // ���� ������
            &cbRead,    // ���ſ� �������� ��� ����Ʈ ���� ��ϵ�
            NULL);      // ��ø I/O ����
    if (!result && GetLastError() != ERROR_MORE_DATA)
    {
        wprintf(TEXT("ReadFile from pipe failed. GLE=%d\n"), GetLastError());
        return FALSE;
    }

    // �о���� ������ ���
    PIPE_MSG* pm = (PIPE_MSG*)buf;
    switch (pm->type)
    {
    case PIPE_TYPE_INIT:
        printf("[SLAVE SERVER:PIPE] %s\n", pm->msg);
        break;
    case PIPE_TYPE_MP:
        // �ް����� ��� ��� ���� ������ ����
        for (int i = 0; i < si_cursor; ++i)
        {
            SOCKET_INFO* ptr = g_SocketInfoArray[i];
            if (!SendChat(pm->msg, &ptr->sock))
            {
                err_display("MP_SendChat()");
                return FALSE;
            }
        }
        break;
    }
    return TRUE;
}

BOOL SendChat(const char* chat, SOCKET* sock)
{
    int retval;
    int len = (int)strlen(chat) + 1;

    MSG_HEADER header = { 0 };
    header.type = TYPE_CHAT;
    header.length = len;

    retval = send(*sock, (char*)&header, sizeof(MSG_HEADER), 0);
    if (SOCKET_ERROR == retval)
    {
        printf("[SLAVE SERVER] ä�� �޽��� ��� ���ۿ� �����߽��ϴ�.\n");
        return false;
    }

    retval = send(*sock, chat, len, 0);
    if (SOCKET_ERROR == retval)
    {
        printf("[SLAVE SERVER] ä�� �޽��� ���ۿ� �����߽��ϴ�.\n");
        return false;
    }

    return true;
}

BOOL SendTextFile(const char* path, SOCKET* sock)
{
    int retval;
    FILE* fp = fopen(path, "rb");
    if (NULL == fp)
    {
        printf("���� ���� ����\n");
        return false;
    }

    int fpos = fseek(fp, 0, SEEK_END);
    long fileSize = ftell(fp);
    rewind(fp);

    MSG_HEADER header = { 0 };
    header.type = TYPE_TXT;
    header.length = fileSize;
    strcpy(header.path, path);

    retval = send(*sock, (char*)&header, sizeof(MSG_HEADER), 0);
    if (SOCKET_ERROR == retval)
    {
        printf("���� ��� ���� ����\n");
        return false;
    }

    char* buffer = (char*)calloc(fileSize, sizeof(char));
    if (NULL == buffer)
    {
        printf("���� ���� �Ҵ� ����\n");
        return false;
    }

    int res = fread(buffer, sizeof(char), fileSize, fp);
    retval = send(*sock, buffer, fileSize, 0);
    if (SOCKET_ERROR == retval)
    {
        printf("���� �ٵ� ���� ����\n");
        return false;
    }

    printf("[TCP SERVER] �ؽ�Ʈ ���� %d / %d ����Ʈ ���� �Ϸ�\n", res, retval);
    fclose(fp);
    free(buffer);
    return true;
}

BOOL SendImageFile(const char* path, SOCKET* sock)
{
    int retval;
    FILE* fp = fopen(path, "rb");
    fseek(fp, 0, SEEK_END);
    long fileSize = ftell(fp);
    rewind(fp);

    MSG_HEADER header = { 0 };
    header.type = TYPE_IMG;
    header.length = fileSize;
    strcpy(header.path, path);

    retval = send(*sock, (char*)&header, sizeof(MSG_HEADER), 0);
    if (SOCKET_ERROR == retval)
    {
        printf("�̹��� ��� ���� ����\n");
        return false;
    }

    char* buffer = (char*)calloc(fileSize, sizeof(char));
    if (NULL == buffer)
    {
        printf("���� ���� �Ҵ� ����\n");
        return false;
    }

    int res = fread(buffer, sizeof(char), fileSize, fp);
    retval = send(*sock, buffer, fileSize, 0);
    if (SOCKET_ERROR == retval)
    {
        printf("���� �۽� ����\n");
        return false;
    }
    printf("[SLAVE SERVER] �̹��� %d / %d����Ʈ ���� �Ϸ�\n", res, retval);
    fclose(fp);
    free(buffer);
    return true;
}

BOOL SendFilePath(const char* path, SOCKET* sock)
{
    int retval;
    MSG_HEADER header;
    header.type = TYPE_FILEPATH;
    header.length = (int)strlen(path) + 1;
    strcpy(header.path, path);
    
    retval = send(*sock, (char*)&header, sizeof(MSG_HEADER), 0);
    if (SOCKET_ERROR == retval)
    {
        err_display("FilePath Send()");
        return FALSE;
    }

    return TRUE;
}

DWORD SlaveMain(LPVOID arg)
{
    int test;
    SOCKET sock = (SOCKET)arg;
    struct sockaddr_in clientaddr;
    int addrlen;
    int retval;

    addrlen = sizeof(clientaddr);
    getpeername(sock, (struct sockaddr*)&clientaddr, &addrlen);
    char addr[INET_ADDRSTRLEN];
    inet_ntop(AF_INET, &clientaddr.sin_addr, addr, sizeof(addr));

    while (true) 
    {
        char* hbuf = (char*)calloc(1, sizeof(MSG_HEADER));
        retval = recv(sock, hbuf, sizeof(MSG_HEADER), 0);
        if (SOCKET_ERROR == retval)
        {
            err_display("header recv()");
            continue;
        }
        else if (0 == retval) break;
        
        MSG_HEADER* received = (MSG_HEADER*)hbuf;
        if (received->type == -1) break;

        printf("type = %d\nlength = %d\npath = %s\n", received->type, received->length, received->path);
        if (!((received->type == TYPE_CHAT) || (received->type == TYPE_TXT) || (received->type == TYPE_IMG) || (received->type == TYPE_TXT_REQ) || (received->type == TYPE_IMG_REQ))) continue;   ////////// ����!

        char* bbuf = (char*)calloc(received->length + 1, sizeof(char));
        if (NULL == bbuf)
        {
            err_display("Failed allocat body buffer\n");
            break;
        }
        
        int idx;
        String cut, savepath;
        FILE* fp;
        int sum = 0; 
        switch (received->type) 
        {
        case TYPE_CHAT:
        {
            retval = recv(sock, bbuf, received->length, 0);
            if (retval == SOCKET_ERROR) err_quit("body recv()");

            int len = (int)strlen(bbuf);
            bbuf[len] = '\0';
            printf("[SLAVE SERVER] %d����Ʈ�� �޾ҽ��ϴ�.\n", retval);
            printf("[SLAVE SERVER] [%s:%d] %s\n", addr, ntohs(clientaddr.sin_port), bbuf);

            /* ��� Ŭ�� ������ */
            for (int i = 0; i < si_cursor; ++i)
            {
                SOCKET_INFO* ptr = g_SocketInfoArray[i];
                if (!SendChat(bbuf, &ptr->sock))
                {
                    err_display("SendChat()");
                    break;
                }
            }
        }
            break;
        case TYPE_TXT:
        {
            idx = lastindexof(received->path, String("\\"));
            cut = substring(received->path, idx + 1, length(received->path) + 1);
            savepath = concat(String(SERVERSTORAGE), cut);
            printf("com path : %s\n", savepath);

            fp = fopen(savepath, "w");
            do {
                char headBuf[HEADSIZE];
                retval = recv(sock, headBuf, sizeof(MSG_HEADER), 0);
                MSG_HEADER* temphd = (MSG_HEADER*)headBuf;
                if (temphd->length == -1) break;

                char* strbuf = (char*)calloc(temphd->length + 1, sizeof(char));
                if (NULL == strbuf)
                {
                    err_display("Failed allocat strbuf.\n");
                    break;
                }

                retval = recv(sock, strbuf, temphd->length, 0);
                printf("%s", strbuf);
                fputs(strbuf, fp);
                free(strbuf);

                sum += retval;
            } while (retval != 0);
            fclose(fp);
            printf("������ ����Ǿ����ϴ�.\n");

            /* ��� Ŭ�� ������ */
            for (int i = 0; i < si_cursor; ++i)
            {
                SOCKET_INFO* ptr = g_SocketInfoArray[i];
                if (!SendFilePath(savepath, &ptr->sock))
                {
                    err_display("SendTextFile()");
                    break;
                }
            }
        }
            break;
        case TYPE_IMG:
        {
            idx = lastindexof(received->path, String("\\"));
            cut = substring(received->path, idx + 1, length(received->path) + 1);
            savepath = concat(String(SERVERSTORAGE), cut);
            printf("com path : %s\n", savepath);

            fp = fopen(savepath, "wb");
            do {
                char tempbuf[HEADSIZE];
                retval = recv(sock, tempbuf, sizeof(MSG_HEADER), 0);
                MSG_HEADER* temphd = (MSG_HEADER*)tempbuf;
                if (temphd->length == -1) break;

                char* imgbuf = (char*)calloc(temphd->length, sizeof(char));
                if (NULL == imgbuf)
                {
                    err_display("Failed allocate imgbuf\n");
                    break;
                }

                retval = recv(sock, imgbuf, temphd->length, 0);
                if (SOCKET_ERROR == retval) break;

                fwrite(imgbuf, sizeof(char), temphd->length, fp);
                free(imgbuf);
                sum += retval;
            } while (retval == HEADSIZE);
            fclose(fp);
            printf("�̹����� ����Ǿ����ϴ�.\n");

            /* ��� Ŭ�� ������ */
            for (int i = 0; i < si_cursor; ++i)
            {
                SOCKET_INFO* ptr = g_SocketInfoArray[i];
                if (!SendFilePath(savepath, &ptr->sock))
                {
                    err_display("SendImageFile()");
                    break;
                }
            }
        }
            break;
        case TYPE_MP:
        {
            // 1. �ٵ� �ޱ�
            retval = recv(sock, bbuf, received->length, 0);
            if (SOCKET_ERROR == retval) err_quit("body recv()");

            int len = (int)strlen(bbuf);
            bbuf[len] = '\0';
            printf("[SLAVE SERVER] %d����Ʈ�� �޾ҽ��ϴ�.\n", retval);
            printf("[SLAVE SERVER] [%s:%d] %s\n", addr, ntohs(clientaddr.sin_port), bbuf);

            std::cin >> test;

            // 1. ������ ���
            PIPE_MSG pm;
            pm.type = PIPE_TYPE_MP;
            strcpy(pm.userName, received->userName);
            strcpy(pm.msg, bbuf);
            DWORD writebytes = PIPE_MSG_SIZE;
            if (!WritePipeMessage(serverInfo->hPipe, (LPVOID)&pm, writebytes, 0))
                err_display("MP WritePipeMsg()");
        }
            break;
        case TYPE_IMG_REQ:
            SendImageFile(received->path, &sock);
            break;
        case TYPE_TXT_REQ:
            SendTextFile(received->path, &sock);
            break;
        }
        
        if (bbuf)
        {
            free(bbuf);
            bbuf = 0;
        }
        
        if (hbuf)
        {
            free(hbuf);
            hbuf = 0;
        }
    }

    RemoveSocketInfo(&sock);
    printf("[SLAVE SERVER] Ŭ���̾�Ʈ ����! [%s:%d]\n", addr, ntohs(clientaddr.sin_port));
    return 0;
}
//
//DWORD ReadPipeMain(LPVOID arg)
//{
//    HANDLE hPipe = (HANDLE)arg;
//    PIPE_MSG pm;
//    DWORD readBytes = PIPE_MSG_SIZE;
//    while (ReadPipeMessage(hPipe, &pm, readBytes, 0));
//
//    printf("������ ������ ����\n");
//    return 0;
//}

void err_quit(const char* title) {
    LPVOID lpMsgBuf;
    FormatMessageA(
        FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ALLOCATE_BUFFER,
        NULL, WSAGetLastError(),
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (char*)&lpMsgBuf, 0, NULL);
    MessageBoxA(NULL, (char*)&lpMsgBuf, title, MB_ICONERROR);
    LocalFree(lpMsgBuf);
    exit(1);
}

void err_display(const char* title) {
    LPVOID lpMsgBuf;
    FormatMessageA(FORMAT_MESSAGE_FROM_SYSTEM | FORMAT_MESSAGE_ALLOCATE_BUFFER,
        NULL, WSAGetLastError(),
        MAKELANGID(LANG_NEUTRAL, SUBLANG_DEFAULT),
        (char*)&lpMsgBuf, 0, NULL);
    printf("[%s] %s\n", title, (char*)&lpMsgBuf);
    LocalFree(lpMsgBuf);
    exit(1);
}