
pipeline {
    agent any
    stages {
        stage('Checkout') {
            steps { git 'https://github.com/yourusername/AutoGladiators.git' }
        }
        stage('Build DLLs') {
            steps { bat 'dotnet build SharedLibs/ -c Release' }
        }
        stage('Build Client') {
            steps { bat 'dotnet build AutoGladiators.Client/ -c Release' }
        }
        stage('Run Tests') {
            steps { bat 'dotnet test Tests/' }
        }
        stage('Package Artifacts') {
            steps { bat 'dotnet publish AutoGladiators.Client/ -c Release -o publish/' }
        }
    }
}
