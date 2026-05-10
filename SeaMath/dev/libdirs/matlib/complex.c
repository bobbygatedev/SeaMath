#include "sea_lib_only.h"

DLL_EXPORT double crealcd(seacmpd z) { return ((seacmpsd*)&z)->re; }

DLL_EXPORT double cimagcd(seacmpd z) { return ((seacmpsd*)&z)->im; }

DLL_EXPORT float crealcf(seacmpf z) { return ((seacmpsf*)&z)->re; }

DLL_EXPORT float cimagcf(seacmpf z) { return ((seacmpsf*)&z)->im; }

DLL_EXPORT int8_t crealci8(seaci8 z) { return ((seacsi8*)&z)->re; }

DLL_EXPORT int8_t cimagci8(seaci8 z) { return ((seacsi8*)&z)->im; }

DLL_EXPORT int16_t crealci16(seaci16 z) { return ((seacsi16*)&z)->re; }

DLL_EXPORT int16_t cimagci16(seaci16 z) { return ((seacsi16*)&z)->im; }

DLL_EXPORT int32_t crealci32(seaci32 z) { return ((seacsi32*)&z)->re; }

DLL_EXPORT int32_t cimagci32(seaci32 z) { return ((seacsi32*)&z)->im; }

DLL_EXPORT int64_t crealci64(seaci64 z) { return ((seacsi64*)&z)->re; }

DLL_EXPORT int64_t cimagci64(seaci64 z) { return ((seacsi64*)&z)->im; }

DLL_EXPORT uint8_t crealcu8(seacu8 z) { return ((seacsu8*)&z)->re; }

DLL_EXPORT uint8_t cimagcu8(seacu8 z) { return ((seacsu8*)&z)->im; }

DLL_EXPORT uint16_t crealcu16(seacu16 z) { return ((seacsu16*)&z)->re; }

DLL_EXPORT uint16_t cimagcu16(seacu16 z) { return ((seacsu16*)&z)->im; }

DLL_EXPORT uint32_t crealcu32(seacu32 z) { return ((seacsu32*)&z)->re; }

DLL_EXPORT uint32_t cimagcu32(seacu32 z) { return ((seacsu32*)&z)->im; }

DLL_EXPORT uint64_t crealcu64(seacu64 z) { return ((seacsu64*)&z)->re; }

DLL_EXPORT uint64_t cimagcu64(seacu64 z) { return ((seacsu64*)&z)->im; }

DLL_EXPORT sealdbl crealcl(seacmpld z) { return ((seacmpsld*)&z)->re; }

DLL_EXPORT sealdbl cimagcl(seacmpld z) { return ((seacmpsld*)&z)->im; }


seacmpd conj(seacmpd z);

seacmpf conjf(seacmpf z);

double carg(seacmpd z);

float cargf(seacmpf z);

seacmpd cproj(seacmpd z);

seacmpf cprojf(seacmpf z);

DLL_EXPORT seacmpd conjcd(seacmpd z) { return conj(z); }

DLL_EXPORT seacmpf conjcf(seacmpf z) { return conjf(z); }

DLL_EXPORT double cargcd(seacmpd z) { return carg(z); }

DLL_EXPORT float cargcf(seacmpf z) { return cargf(z); }

DLL_EXPORT seacmpd cprojcd(seacmpd z) { return cproj(z); }

DLL_EXPORT seacmpf cprojcf(seacmpf z) { return cprojf(z); }
