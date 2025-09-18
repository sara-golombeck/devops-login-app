# זרימת המערכת - Automarkly Email Service

## דיאגרמת זרימה מלאה

```mermaid
graph TD
    A[משתמש נכנס ל-MYNAME.CLICK] --> B[CloudFront CDN]
    B --> C[React Frontend<br/>S3 Static Website]
    
    C --> D[משתמש ממלא טופס אימייל]
    D --> E[Frontend שולח POST request]
    E --> F[AWS Load Balancer]
    F --> G[.NET Backend API<br/>EKS Pod]
    
    G --> H{אימות נתונים}
    H -->|תקין| I[שמירה ב-PostgreSQL]
    H -->|לא תקין| J[החזרת שגיאה למשתמש]
    
    I --> K[יצירת הודעה ל-SQS]
    K --> L[AWS SQS Queue<br/>email-processing-queue]
    
    L --> M[Email Worker<br/>EKS Pod]
    M --> N[קריאת הודעה מ-SQS]
    N --> O[עיבוד נתוני האימייל]
    O --> P[שליחה דרך AWS SES]
    
    P --> Q{האם נשלח בהצלחה?}
    Q -->|כן| R[עדכון סטטוס ב-DB]
    Q -->|לא| S[Retry Logic<br/>או Dead Letter Queue]
    
    R --> T[מחיקת הודעה מ-SQS]
    S --> U[ניסיון חוזר או כישלון סופי]
    
    J --> V[תגובה למשתמש]
    T --> W[אימייל נשלח בהצלחה]
    U --> X[התראה על כישלון]

    style A fill:#e1f5fe
    style C fill:#f3e5f5
    style G fill:#e8f5e8
    style L fill:#fff3e0
    style M fill:#e8f5e8
    style P fill:#ffebee
```

## זרימה מפורטת צעד אחר צעד

### 1. כניסת המשתמש
```mermaid
sequenceDiagram
    participant U as משתמש
    participant CF as CloudFront
    participant S3 as S3 Bucket
    participant R as React App
    
    U->>CF: גישה ל-MYNAME.CLICK
    CF->>S3: בקשת קבצים סטטיים
    S3->>CF: HTML, CSS, JS
    CF->>U: דף האתר
    Note over U: המשתמש רואה טופס אימייל
```

### 2. שליחת אימייל
```mermaid
sequenceDiagram
    participant R as React Frontend
    participant ALB as AWS Load Balancer
    participant API as .NET Backend
    participant DB as PostgreSQL
    participant SQS as AWS SQS
    
    R->>ALB: POST /api/emails
    ALB->>API: העברת בקשה
    API->>API: אימות נתונים
    API->>DB: שמירת פרטי אימייל
    DB->>API: אישור שמירה
    API->>SQS: שליחת הודעה לתור
    SQS->>API: אישור קבלה
    API->>R: תגובה - "האימייל בתהליך שליחה"
```

### 3. עיבוד האימייל
```mermaid
sequenceDiagram
    participant SQS as AWS SQS
    participant W as Email Worker
    participant DB as PostgreSQL
    participant SES as AWS SES
    
    W->>SQS: polling להודעות חדשות
    SQS->>W: הודעת אימייל
    W->>W: עיבוד נתוני האימייל
    W->>SES: שליחת אימייל
    SES->>W: סטטוס שליחה
    W->>DB: עדכון סטטוס
    W->>SQS: מחיקת הודעה מהתור
```

## רכיבי המערכת

### Frontend (React)
- **מיקום**: S3 + CloudFront
- **תפקיד**: ממשק משתמש לשליחת אימיילים
- **טכנולוגיה**: React, TypeScript

### Backend API (.NET)
- **מיקום**: EKS Pods
- **תפקיד**: קבלת בקשות, אימות, שמירה ב-DB, שליחה ל-SQS
- **טכנולוגיה**: .NET 8, ASP.NET Core

### Email Worker
- **מיקום**: EKS Pods
- **תפקיד**: עיבוד אימיילים מ-SQS ושליחה דרך SES
- **טכנולוגיה**: .NET 8, Background Service

### תשתית AWS
- **SQS**: תור הודעות אסינכרוני
- **SES**: שירות שליחת אימיילים
- **PostgreSQL**: מסד נתונים לאחסון
- **EKS**: Kubernetes cluster

## יתרונות הארכיטקטורה

1. **אמינות**: SQS מבטיח שהודעות לא יאבדו
2. **מדרגיות**: Workers יכולים להתרחב אוטומטית
3. **ביצועים**: עיבוד אסינכרוני
4. **ניטור**: Prometheus + Grafana
5. **אבטחה**: IRSA, Network Policies, TLS

## נקודות כישלון אפשריות

- **Retry Logic**: ניסיונות חוזרים על כישלונות זמניים
- **Dead Letter Queue**: הודעות שנכשלו מספר פעמים
- **Health Checks**: בדיקות תקינות רכיבים
- **Circuit Breaker**: הגנה מפני עומסים